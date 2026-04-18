/*jshint esversion: 6 */
/* jshint node: true */
/*jslint browser: true */
/* global logData*/
// const images
"use strict";

function compileCRTemplates() {
    TEMPLATE_CR_COMPILE
};

const noUpdateTime = -1;
const updateText = -2;
const deadIcon = new Image();
deadIcon.onload = function () {
    animateCanvas(noUpdateTime);
};
const downEnemyIcon = new Image();
downEnemyIcon.onload = function () {
    animateCanvas(noUpdateTime);
};
const downAllyIcon = new Image();
downAllyIcon.onload = function () {
    animateCanvas(noUpdateTime);
};
const dcIcon = new Image();
dcIcon.onload = function () {
    animateCanvas(noUpdateTime);
};
const facingIcon = new Image();
facingIcon.onload = function () {
    animateCanvas(noUpdateTime);
};

function ToRadians(degrees) {
    return degrees * (Math.PI / 180);
}
function ToDegrees(radians) {
    return radians / (Math.PI / 180);
}

const resolutionMultiplier = 2.0;

const maxOverheadAnimationFrame = 50;
let overheadAnimationFrame = maxOverheadAnimationFrame / 2;
let overheadAnimationIncrement = 1;

const uint32 = new Uint32Array(1);
const uint32ToUint8 = new Uint8Array(uint32.buffer);


// Define the type of the decoration. Must match ordering of the enum in CombatReplayDescription.cs
const Types = {
    ActorOrientation: 0,
    BackgroundIcon: 1,
    Circle: 2,
    Doughnut: 3,
    Friendly: 4,
    FriendlyPlayer: 5,
    Icon: 6,
    IconOverhead: 7,
    Line: 8,
    Mob: 9,
    MovingPlatform: 10,
    Pie: 11,
    Player: 12,
    ProgressBar: 13,
    ProgressBarOverhead: 14,
    Rectangle: 15,
    SquadMarker: 16,
    SquadMarkerOverhead: 17,
    Target: 18,
    TargetPlayer: 19,
    Text: 20,
    Polygon: 21,
    TextOverhead: 22,
    Arena: 23,
};

function getDefaultCombatReplayTime() {
    var time = EIUrlParams.get("crTime");
    if (!time) {
        return 0;
    }
    return Math.max(parseFloat(time), 0.0) * 1000;
}

var animator = null;
// reactive structures
const reactiveAnimationData = {
    time: getDefaultCombatReplayTime(),
    selectedActorID: null,
    hoveredActorKind: null,
    hoveredActorID: null,
    hoveredActorX: 0,
    hoveredActorY: 0,
    animated: false,
    range: {
        min: 0,
        max: 1e12
    }
};

var sliderDelimiter = {
    min: -1,
    max: -1,
    name: logData.phases[0].name
}
//

let InchToPixel = 10;
let PollingRate = 150;

// Scenegraph

function standardDraw(drawable) {
    drawable.draw();
}

function selectableDraw(drawable) {
    if (!drawable.isSelected()) {
        drawable.draw();
        animator._drawActorOrientation(drawable.id);
    }
}

function selectablePickingDraw(drawable) {
    if (!drawable.isSelected()) {
        drawable.drawPicking();
    }
}

class RenderablesBranch {
    constructor(start, end) {
        this.start = start;
        this.end = end;
        this.halfPoint = (end - start) * 0.5 + start;
        this.left = null;
        this.right = null;
        this.renderables = [];
        this.leaf = true;
        // Won't allow leaf below this
        this.finalLeaf = this.end - this.start < 10000;
    }

    add(item) {
        if (this.leaf) {
            this.renderables.push(item);
            // If too many renderables, remove leaf and redistribute
            if (this.renderables.length > 50 && !this.finalLeaf) {
                this.leaf = false;
                const renderablesToRedistribute = this.renderables;
                this.renderables = [];
                for (let i = 0; i < renderablesToRedistribute.length; i++) {
                    this.add(renderablesToRedistribute[i]);
                }
            }
            return;
        }
        if (item.end <= this.halfPoint) {
            if (!this.left) {
                this.left = new RenderablesBranch(this.start, this.halfPoint);
            }
            this.left.add(item);
        } else if (item.start > this.halfPoint && item.end <= this.end) {
            if (!this.right) {
                this.right = new RenderablesBranch(this.halfPoint, this.end);
            }
            this.right.add(item);
        } else {
            this.renderables.push(item);
        }
    }

    forEach(cb) {  
        for (let i = 0; i < this.renderables.length; i++) {
            cb(this.renderables[i]);
        }
        if (this.left) {
            this.left.forEach(cb);
        }
        if (this.right) {
            this.right.forEach(cb);
        }
    }

    draw(drawFunction) {
        var time = animator.reactiveDataStatus.time;
        if (this.start > time || this.end < time) {
            return;
        }
        for (let i = 0; i < this.renderables.length; i++) {
            drawFunction(this.renderables[i]);
        }
        if (this.left) {
            this.left.draw(drawFunction);
        }
        if (this.right) {
            this.right.draw(drawFunction);
        }
    }

    any()  {
        return this.renderables.length > 0 || this.left || this.right;
    }
}

class RenderablesRoot extends RenderablesBranch{
    constructor(start, end) {
        super(start, end);
        this._allRenderables = [];
    }

    add(item) {
        super.add(item);
        this._allRenderables.push(item);
    }
}

class MappedRenderablesRoot extends RenderablesRoot {
    constructor(start, end) {
        super(start, end);
        this.map = new Map();
    }

    add(item) {
        super.add(item);
        this.map.set(item.id, item);
    }

    get(id) {
        return this.map.get(id);
    }
    
    has(id) {
        return this.map.has(id);
    }
}

//

class Animator {
    constructor(options) {
        var _this = this;
        // status
        this.reactiveDataStatus = reactiveAnimationData;
        // time
        this.prevTime = 0;
        this.times = [];
        // simulation params
        this.speed = 1;
        this.backwards = false;
        this.rangeControl = [{ enabled: false, radius: 180 }, { enabled: false, radius: 360 }, { enabled: false, radius: 720 }];
        this.displaySettings = {
            highlightSelectedGroup: true,
            displayAllMinions: false,
            displaySelectedMinions: true,
            displayMechanics: true,
            displaySquadMarkers: true,
            displaySkillMechanics: true,
            skillMechanicsMask: DefaultSkillDecorations,
            displayTrashMobs: true,
            useActorHitboxWidth: false,
            followSelected: false
        };
        this.coneControl = {
            enabled: false,
            openingAngle: 90,
            radius: 360,
        };
        // actors
        const start = logData.phases[0].start * 1000;
        const end = logData.phases[0].end * 1000;
        this.targetData = new MappedRenderablesRoot(start, end);
        this.targetPlayerData = new MappedRenderablesRoot(start, end);
        this.playerData = new MappedRenderablesRoot(start, end);
        this.trashMobData = new MappedRenderablesRoot(start, end);
        this.friendlyMobData = new MappedRenderablesRoot(start, end);
        this.friendlyPlayerData = new MappedRenderablesRoot(start, end);
        this.decorationMetadata = new Map();
        this.overheadActorData = new RenderablesRoot(start, end);
        this.squadMarkerData = new RenderablesRoot(start, end);
        this.overheadSquadMarkerData = new RenderablesRoot(start, end);
        this.mechanicActorData = new RenderablesRoot(start, end);
        this.skillMechanicActorData = new RenderablesRoot(start, end);
        this.actorOrientationData = new Map();
        this.backgroundActorData = [];
        this.screenSpaceActorData = new RenderablesRoot(start, end);
        this.agentDataPerParentID = new Map();
        this.selectedActor = null;
        // maps
        this.backgroundImages = new RenderablesRoot(start, end);
        // animation
        this.needBGUpdate = false;
        this.animation = null;
        // manipulation
        this.mouseDown = null;
        this.dragged = false;
        this.scale = 1.0;
        this.hoveredActor = null;
        this.hoveredActorLabel = "";
        this.hoveredActorScreenPosition = null;
        this.hoverDelay = 250;
        this.hoverTimer = null;
        this.pendingHoveredActor = null;
        this.pendingHoveredActorScreenPosition = null;
        // options
        if (options) {
            if (options.inchToPixel) {
                InchToPixel = options.inchToPixel;
            }
            if (options.pollingRate) {
                PollingRate = options.pollingRate;
            }
            if (options.actors) {
                this._initActors(options.actors, options.decorationRenderings, options.decorationMetadata);
            }
            if (!replaceImgur) {
                downEnemyIcon.crossOrigin = "Anonymous";
                downAllyIcon.crossOrigin = "Anonymous";
                dcIcon.crossOrigin = "Anonymous";
                deadIcon.crossOrigin = "Anonymous";
            }
            downEnemyIcon.src = UIIcons.DownedEnemy;
            downAllyIcon.src = UIIcons.DownedAlly;
            dcIcon.src = UIIcons.Disconnected;
            deadIcon.src = UIIcons.Dead;
            facingIcon.src = UIIcons.Facing;
        }
        let cur = start;
        while (cur < end) {
            this.times.push(cur);
            cur += PollingRate;
        }
        this.reactiveDataStatus.time = start;
        this.reactiveDataStatus.range.min = this.times[0];
        this.reactiveDataStatus.range.max = this.times[this.times.length - 1];
    }

    attachDOM(mainCanvasID, bgCanvasID, pickCanvasID, timeRangeID, timeRangeDisplayID) {
        // animation
        this.timeSlider = document.getElementById(timeRangeID);
        this.timeSliderDisplay = document.getElementById(timeRangeDisplayID);
        // main canvas
        this.mainCanvas = document.getElementById(mainCanvasID);
        this.mainCanvas.style.width = this.mainCanvas.width + "px";
        this.mainCanvas.style.height = this.mainCanvas.height + "px";
        this.mainCanvas.width *= resolutionMultiplier;
        this.mainCanvas.height *= resolutionMultiplier;
        this.mainContext = this.mainCanvas.getContext('2d');
        this.mainContext.imageSmoothingEnabled = true;
        // bg canvas
        this.bgCanvas = document.getElementById(bgCanvasID);
        this.bgCanvas.style.width = this.bgCanvas.width + "px";
        this.bgCanvas.style.height = this.bgCanvas.height + "px";
        this.bgCanvas.width *= resolutionMultiplier;
        this.bgCanvas.height *= resolutionMultiplier;
        this.bgContext = this.bgCanvas.getContext('2d');
        this.bgContext.imageSmoothingEnabled = true;
        // pick canvas
        this.pickCanvas = document.getElementById(pickCanvasID);
        this.pickCanvas.style.width = this.pickCanvas.width + "px";
        this.pickCanvas.style.height = this.pickCanvas.height + "px";
        this.pickCanvas.width *= resolutionMultiplier;
        this.pickCanvas.height *= resolutionMultiplier;
        this.pickContext = this.pickCanvas.getContext('2d', {
            willReadFrequently: true,
        });
        // manipulation
        this.lastX = this.mainCanvas.width / 2;
        this.lastY = this.mainCanvas.height / 2;
        //
        this._trackTransforms(this.mainContext);
        this._trackTransforms(this.bgContext);
        this._trackTransforms(this.pickContext);
        this.mainContext.scale(resolutionMultiplier, resolutionMultiplier);
        this.bgContext.scale(resolutionMultiplier, resolutionMultiplier);
        this.pickContext.scale(resolutionMultiplier, resolutionMultiplier);
        // Fresh canvas elements need a full background redraw after the DOM is reattached.
        this.needBGUpdate = true;
        this._initMouseEvents();
        this._initTouchEvents();
    }

    _initActors(actors, decorationRenderings, decorationMetadata) {
        for (let i = 0; i < decorationMetadata.length; i++) {
            const metadata = decorationMetadata[i];
            let MetadataClass = null;
            switch (metadata.type) {
                case Types.ActorOrientation:
                    MetadataClass = ActorOrientationMetadata;
                    break;
                case Types.Circle:
                    MetadataClass = CircleMetadata;
                    break;
                case Types.Polygon:
                    MetadataClass = PolygonMetadata;
                    break;
                case Types.Doughnut:
                    MetadataClass = DoughnutMetadata;
                    break;
                case Types.Line:
                    MetadataClass = LineMetadata;
                    break;
                case Types.Pie:
                    MetadataClass = PieMetadata;
                    break;
                case Types.Rectangle:
                    MetadataClass = RectangleMetadata;
                    break;
                case Types.ProgressBar:
                    MetadataClass = ProgressBarMetadata;
                    break;
                case Types.BackgroundIcon:
                    MetadataClass = IconMetadata;
                    break;
                case Types.Icon:
                    MetadataClass = IconMetadata;
                    break;
                case Types.IconOverhead:
                    MetadataClass = IconOverheadMetadata;
                    break;
                case Types.ProgressBarOverhead:
                    MetadataClass = OverheadProgressBarMetadata;
                    break;
                case Types.MovingPlatform:
                    MetadataClass = MovingPlatformMetadata;
                    break;
                case Types.Text:
                    MetadataClass = TextMetadata;
                    break;
                case Types.TextOverhead:
                    MetadataClass = TextOverheadMetadata;
                    break;
                case Types.Arena:
                    MetadataClass = ArenaMetadata;
                    break;
                default:
                    throw "Unknown decoration type " + metadata.type;
            }
            this.decorationMetadata.set(metadata.signature, new MetadataClass(metadata));
        }
        for (let i = 0; i < actors.length; i++) {
            const actor = actors[i];
            let ActorClass;
            let actorSize = 0;
            let mapToFill;
            switch (actor.type) {
                case Types.Player:
                    ActorClass = PlayerIconDrawable;
                    actorSize = 22;
                    mapToFill = this.playerData;
                    break;
                case Types.Target:
                    ActorClass = NPCIconDrawable;
                    actorSize = 30;
                    mapToFill = this.targetData;
                    break;
                case Types.TargetPlayer:
                    ActorClass = EnemyPlayerDrawable;
                    actorSize = 22;
                    mapToFill = this.targetPlayerData;
                    break;
                case Types.Mob:
                    ActorClass = NPCIconDrawable;
                    actorSize = 25;
                    mapToFill = this.trashMobData;
                    break;
                case Types.Friendly:
                    ActorClass = NPCIconDrawable;
                    actorSize = 22;
                    mapToFill = this.friendlyMobData;
                    break;
                case Types.FriendlyPlayer:
                    ActorClass = FriendlyPlayerDrawable;
                    actorSize = 22;
                    mapToFill = this.friendlyPlayerData;
                    break;
                default:
                    throw "Unknown decoration type " + actor.type;
            }
            const renderable = new ActorClass(actor, actorSize);
            mapToFill.add(renderable);
            if (renderable.parentID >= 0) {
                let array = this.agentDataPerParentID.get(renderable.parentID) ?? [];
                array.push(renderable);
                this.agentDataPerParentID.set(renderable.parentID, array);
            }
        }
        for (let i = 0; i < decorationRenderings.length; i++) {
            const decorationRendering = {};
            decorationRendering._metadataContainer = this.decorationMetadata;
            Object.assign(decorationRendering, decorationRenderings[i]);
            if (!decorationRendering.isMechanicOrSkill) {
                switch (decorationRendering.type) {
                    case Types.ActorOrientation:
                        let orientationID = decorationRendering.connectedTo.masterID;
                        var orientationDrawable = new ActorOrientationDrawable(decorationRendering);
                        if (this.agentDataPerParentID.has(orientationID)) {
                            let halfTime = (orientationDrawable.start + orientationDrawable.end) / 2;
                            let agents = this.agentDataPerParentID.get(orientationID);
                            for (let i = 0; i < agents.length; i++) {
                                let agent = agents[i];
                                if (agent.start <= halfTime && agent.end >= halfTime) {
                                    this.actorOrientationData.set(agents[i].id, orientationDrawable);
                                    break;
                                }
                            }
                        } else {
                            this.actorOrientationData.set(orientationID, orientationDrawable);
                        }
                        break;
                    case Types.MovingPlatform:
                        this.backgroundActorData.push(new MovingPlatformDrawable(decorationRendering));
                        break;
                    case Types.BackgroundIcon:
                        this.backgroundActorData.push(new BackgroundIconMechanicDrawable(decorationRendering));
                        break;
                    case Types.Arena:
                        this.backgroundImages.add(new ArenaDrawable(decorationRendering));
                        break;
                    default:
                        throw "Unknown decoration type " + decorationRendering.type;
                }
            } else {
                let DecorationClass;
                switch (decorationRendering.type) {
                    case Types.Text:
                        if (decorationRendering.connectedTo.isScreenSpace) {
                            this.screenSpaceActorData.add(new TextDrawable(decorationRendering));
                            continue;
                        }
                        DecorationClass = TextDrawable;
                        break;
                    case Types.TextOverhead:
                        this.overheadActorData.add(new TextOverheadDrawable(decorationRendering));
                        continue;
                    case Types.Circle:
                        DecorationClass = CircleMechanicDrawable;
                        break;
                    case Types.Polygon:
                        DecorationClass = PolygonMechanicDrawable;
                        break;
                    case Types.Rectangle:
                        DecorationClass = RectangleMechanicDrawable;
                        break;
                    case Types.ProgressBar:
                        DecorationClass = ProgressBarMechanicDrawable;
                        break;
                    case Types.Doughnut:
                        DecorationClass = DoughnutMechanicDrawable;
                        break;
                    case Types.Pie:
                        DecorationClass = PieMechanicDrawable;
                        break;
                    case Types.Line:
                        DecorationClass = LineMechanicDrawable;
                        break;
                    case Types.Icon:
                        DecorationClass = IconMechanicDrawable;
                        break;
                    case Types.IconOverhead:
                        this.overheadActorData.add(new IconOverheadMechanicDrawable(decorationRendering));
                        continue;
                    case Types.ProgressBarOverhead:
                        this.overheadActorData.add(new OverheadProgressBarMechanicDrawable(decorationRendering));
                        continue;
                    case Types.SquadMarker:
                        this.squadMarkerData.add(new IconMechanicDrawable(decorationRendering));
                        continue;
                    case Types.SquadMarkerOverhead:
                        this.overheadSquadMarkerData.add(new IconOverheadMechanicDrawable(decorationRendering));
                        continue;
                    default:
                        throw "Unknown decoration type " + decorationRendering.type;
                }
                const decoration = new DecorationClass(decorationRendering);
                if (decorationRendering.skillMode) {
                    this.skillMechanicActorData.add(decoration);
                } else {
                    this.mechanicActorData.add(decoration);
                }
            }
        }
    }

    updateRange(phase) {
        let min = Math.max(this.times[0], phase.start * 1000);
        let max = Math.min(this.times[this.times.length - 1], phase.end * 1000);
        this.reactiveDataStatus.range.min = min;
        this.reactiveDataStatus.range.max = max;
    }

    updateTime(value) {
        this.reactiveDataStatus.time = parseInt(value);
        if (this.animation === null) {
            animateCanvas(noUpdateTime);
        }
    }

    updateTextInput() {
        this.timeSliderDisplay.value = ((this.reactiveDataStatus.time - this.reactiveDataStatus.range.min) / 1000.0).toFixed(3);
    }

    updateInputTime(value) {
        try {
            const cleanedString = value.replace(",", ".");
            const parsedTime = parseFloat(cleanedString);
            if (isNaN(parsedTime) || !isFinite(parsedTime)) {
                return;
            }
            const ms = Math.round(parsedTime * 1000.0);
            const min = this.reactiveDataStatus.range.min;
            const max = this.reactiveDataStatus.range.max;
            this.reactiveDataStatus.time = Math.min(Math.max(ms, min), max);
            animateCanvas(updateText);
        } catch (error) {
            console.error(error);
        }
    }

    toggleAnimate() {
        if (!this.startAnimate(true)) {
            this.stopAnimate(true);
        }
    }

    startAnimate(updateReactiveStatus) {
        if (this.animation === null && this.times.length > 0) {
            const max = this.reactiveDataStatus.range.max;
            const min = this.reactiveDataStatus.range.min;
            if (this.reactiveDataStatus.time >= max && !this.backwards) {
                this.reactiveDataStatus.time = min;
            }
            this.prevTime = new Date().getTime();
            this.animation = requestAnimationFrame(animateCanvas);
            if (updateReactiveStatus) {
                this.reactiveDataStatus.animated = true;
            }
            return true;
        }
        return false;
    }

    stopAnimate(updateReactiveStatus) {
        if (this.animation !== null) {
            window.cancelAnimationFrame(this.animation);
            this.animation = null;
            if (updateReactiveStatus) {
                this.reactiveDataStatus.animated = false;
            }
            return true;
        }
        return false;
    }

    restartAnimate() {
        this.reactiveDataStatus.time = this.reactiveDataStatus.range.min;
        if (this.animation === null) {
            animateCanvas(noUpdateTime);
        }
    }

    selectActor(actorId, keepIfEqual = false) {
        if (DEBUG) {
            const inLogActor = logData.players.filter(x => x.uniqueID === actorId)[0] || logData.targets.filter(x => x.uniqueID === actorId)[0];
            if (inLogActor) {
                alert(actorId + " " + inLogActor.name)
            } else {
                alert(actorId);
            }
        }
        let actor = this.getActorData(actorId);
        if (!actor || (!keepIfEqual && this.selectedActor === actor)) {
            this.selectedActor = null;
            this.reactiveDataStatus.selectedActorID = null;
        } else {
            this.selectedActor = actor;
            this.reactiveDataStatus.selectedActorID = actorId;
        }
        if (this.animation === null) {
            animateCanvas(noUpdateTime);
        }
    }

    focusActorAtScale(actorId, unitsAtScale) {
        if (!this.mainCanvas || !this.bgCanvas) {
            return;
        }
        const actor = this.getActorData(actorId);
        if (!actor) {
            return;
        }
        this.selectedActor = actor;
        this.reactiveDataStatus.selectedActorID = actorId;
        this._reselectIfEnglobed();
        const selectedActor = this.selectedActor;
        const pos = selectedActor ? selectedActor.getPosition() : null;
        if (pos === null) {
            if (this.animation === null) {
                animateCanvas(noUpdateTime);
            }
            return;
        }

        const targetScale = unitsAtScale > 0 ? 50 / (InchToPixel * unitsAtScale) : 1.0;
        const canvas = this.mainCanvas;
        const ctx = this.mainContext;
        const bgCtx = this.bgContext;
        this.lastX = canvas.width / 2;
        this.lastY = canvas.height / 2;
        this.mouseDown = null;
        this.dragged = false;
        this.coneControl.enabled = true;

        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.scale(resolutionMultiplier, resolutionMultiplier);
        bgCtx.setTransform(1, 0, 0, 1, 0, 0);
        bgCtx.scale(resolutionMultiplier, resolutionMultiplier);
        ctx.scale(targetScale, targetScale);
        bgCtx.scale(targetScale, targetScale);

        const translateScale = 0.5 / resolutionMultiplier / targetScale;
        ctx.translate(-pos.x + canvas.width * translateScale, -pos.y + canvas.height * translateScale);
        bgCtx.translate(-pos.x + canvas.width * translateScale, -pos.y + canvas.height * translateScale);
        this.needBGUpdate = true;
        if (this.animation === null) {
            animateCanvas(noUpdateTime);
        }
    }
    
    _reselectIfEnglobed() {     
        if (this.selectedActor && this.selectedActor.parentID >= 0) {
            const perParentArray = this.agentDataPerParentID.get(this.selectedActor.parentID);
            if (perParentArray) {
                let actor = perParentArray.filter(x => x.getPosition() != null)[0];
                if (!actor) {
                    const time = this.reactiveDataStatus.time;
                    // check for first in interval
                    let candidates = perParentArray.filter(x => x.start <= time && x.end >= time);
                    if (candidates.length) {
                        actor = candidates[0];
                    } else {
                        // first
                        candidates = perParentArray.filter(x => x.start >= time);
                        if (candidates.length) {
                            actor = candidates[0];
                        } else {
                            // last
                            candidates = perParentArray.filter(x => x.end <= time);
                            if (candidates.length) {
                                actor = candidates[candidates.length - 1];
                            }
                        }
                    }
                }
                this.selectedActor = actor || this.selectedActor;
                this.reactiveDataStatus.selectedActorID = this.selectedActor.id;             
            }
        }
    }

    getSelectableActorData(actorId) {
        return animator.targetData.get(actorId) || animator.playerData.get(actorId) || 
                animator.friendlyMobData.get(actorId) || animator.friendlyPlayerData.get(actorId) || 
                animator.targetPlayerData.get(actorId);
    }

    getActorData(actorId) {
        return this.getSelectableActorData(actorId) || animator.trashMobData.get(actorId);
    }

    getActiveActorMarkers(actorID) {
        let res = [];
        const _this = this;
        this.overheadSquadMarkerData.forEach((marker) => {
            if (marker.canDraw() && marker.getPosition() && marker.master === _this.getActorData(actorID)) {
                res.push(marker);
            }
        });
        return res;
    }

    toggleFollowSelected() {
        this.displaySettings.followSelected = !this.displaySettings.followSelected;
        animateCanvas(noUpdateTime);
    }

    toggleHighlightSelectedGroup() {
        this.displaySettings.highlightSelectedGroup = !this.displaySettings.highlightSelectedGroup;
        animateCanvas(noUpdateTime);
    }

    toggleDisplayAllMinions() {
        this.displaySettings.displayAllMinions = !this.displaySettings.displayAllMinions;
        animateCanvas(noUpdateTime);
    }

    toggleDisplaySelectedMinions() {
        this.displaySettings.displaySelectedMinions = !this.displaySettings.displaySelectedMinions;
        animateCanvas(noUpdateTime);
    }

    toggleUseActorHitboxWidth() {
        this.displaySettings.useActorHitboxWidth = !this.displaySettings.useActorHitboxWidth;
        animateCanvas(noUpdateTime);
    }

    toggleTrashMobs() {
        this.displaySettings.displayTrashMobs = !this.displaySettings.displayTrashMobs;
        animateCanvas(noUpdateTime);
    }

    toggleMechanics() {
        this.displaySettings.displayMechanics = !this.displaySettings.displayMechanics;
        animateCanvas(noUpdateTime);
    }

    toggleSquadMarkers() {
        this.displaySettings.displaySquadMarkers = !this.displaySettings.displaySquadMarkers;
        animateCanvas(noUpdateTime);
    }

    toggleSkills() {
        this.displaySettings.displaySkillMechanics = !this.displaySettings.displaySkillMechanics;
        animateCanvas(noUpdateTime);
    }

    toggleSkillCategoryMask(mask) {
        if ((this.displaySettings.skillMechanicsMask & mask) > 0) {
            this.displaySettings.skillMechanicsMask &= ~mask;
        } else {
            this.displaySettings.skillMechanicsMask |= mask;
        }
        animateCanvas(noUpdateTime);
    }

    toggleConeDisplay() {
        this.coneControl.enabled = !this.coneControl.enabled;
        animateCanvas(noUpdateTime);
    }

    setConeRadius(value) {
        this.coneControl.radius = value;
        animateCanvas(noUpdateTime);
    }

    setConeAngle(value) {
        this.coneControl.openingAngle = value;
        animateCanvas(noUpdateTime);
    }

    resetViewpoint() {
        var canvas = this.mainCanvas;
        var ctx = this.mainContext;
        var bgCtx = this.bgContext;

        this.lastX = canvas.width / 2;
        this.lastY = canvas.height / 2;
        this.mouseDown = null;
        this.dragged = false;
        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.scale(resolutionMultiplier, resolutionMultiplier);
        bgCtx.setTransform(1, 0, 0, 1, 0, 0);
        bgCtx.scale(resolutionMultiplier, resolutionMultiplier);
        this.needBGUpdate = true;
        if (this.animation === null) {
            animateCanvas(noUpdateTime);
        }
    }

    _getActorLabel(actor) {
        if (!actor) {
            return "";
        }
        if (actor.name && actor.name.length > 0) {
            return actor.name;
        }
        const actorId = actor.id;
        const collections = [logData.players, logData.targets, logData.enemies];
        for (let i = 0; i < collections.length; i++) {
            const collection = collections[i];
            if (!collection) {
                continue;
            }
            const match = collection.find(entry => entry && entry.uniqueID === actorId);
            if (match && match.name) {
                return match.name;
            }
        }
        return "";
    }

    _isPlayerActor(actor) {
        if (!actor) {
            return false;
        }
        return !!(logData.players && logData.players.find(entry => entry && entry.uniqueID === actor.id));
    }

    _isTargetActor(actor) {
        if (!actor) {
            return false;
        }
        return !!(logData.targets && logData.targets.find(entry => entry && entry.uniqueID === actor.id));
    }

    _applyHoveredActor(actor, screenX, screenY) {
        const nextLabel = this._getActorLabel(actor);
        const sameActor = this.hoveredActor === actor;
        const sameLabel = this.hoveredActorLabel === nextLabel;
        const samePosition = this.hoveredActorScreenPosition !== null
            && this.hoveredActorScreenPosition.x === screenX
            && this.hoveredActorScreenPosition.y === screenY;
        if (sameActor && sameLabel && samePosition) {
            return;
        }
        this.hoveredActor = actor;
        this.hoveredActorLabel = nextLabel;
        this.hoveredActorScreenPosition = actor && nextLabel ? { x: screenX, y: screenY } : null;
        const isPlayer = actor && this._isPlayerActor(actor);
        const isTarget = actor && this._isTargetActor(actor);
        this.reactiveDataStatus.hoveredActorKind = isPlayer ? "player" : (isTarget ? "target" : null);
        this.reactiveDataStatus.hoveredActorID = actor && (isPlayer || isTarget) ? actor.id : null;
        this.reactiveDataStatus.hoveredActorX = actor ? screenX : 0;
        this.reactiveDataStatus.hoveredActorY = actor ? screenY : 0;
        if (this.animation === null) {
            animateCanvas(noUpdateTime);
        }
    }

    _scheduleHoveredActor(actor, screenX, screenY) {
        if (!actor) {
            this._clearHoveredActor();
            return;
        }
        if (this.hoveredActor !== null && this.hoveredActor !== actor) {
            this._applyHoveredActor(null, 0, 0);
        }
        if (this.hoveredActor === actor) {
            this._applyHoveredActor(actor, screenX, screenY);
            return;
        }
        if (this.pendingHoveredActor === actor && this.hoverTimer !== null) {
            this.pendingHoveredActorScreenPosition = { x: screenX, y: screenY };
            return;
        }
        if (this.hoverTimer !== null) {
            clearTimeout(this.hoverTimer);
            this.hoverTimer = null;
        }
        this.pendingHoveredActor = actor;
        this.pendingHoveredActorScreenPosition = { x: screenX, y: screenY };
        this.hoverTimer = window.setTimeout(() => {
            this.hoverTimer = null;
            const pendingActor = this.pendingHoveredActor;
            const pendingPosition = this.pendingHoveredActorScreenPosition;
            this.pendingHoveredActor = null;
            this.pendingHoveredActorScreenPosition = null;
            if (!pendingActor) {
                return;
            }
            this._applyHoveredActor(pendingActor, pendingPosition ? pendingPosition.x : screenX, pendingPosition ? pendingPosition.y : screenY);
        }, this.hoverDelay);
    }

    _clearHoveredActor() {
        if (this.hoverTimer !== null) {
            clearTimeout(this.hoverTimer);
            this.hoverTimer = null;
        }
        this.pendingHoveredActor = null;
        this.pendingHoveredActorScreenPosition = null;
        if (this.hoveredActor === null && this.hoveredActorLabel.length === 0 && this.hoveredActorScreenPosition === null) {
            return;
        }
        this.hoveredActor = null;
        this.hoveredActorLabel = "";
        this.hoveredActorScreenPosition = null;
        this.reactiveDataStatus.hoveredActorKind = null;
        this.reactiveDataStatus.hoveredActorID = null;
        this.reactiveDataStatus.hoveredActorX = 0;
        this.reactiveDataStatus.hoveredActorY = 0;
        if (this.animation === null) {
            animateCanvas(noUpdateTime);
        }
    }

    _pickActorAtScreenPoint(screenX, screenY) {
        if (!this.pickContext) {
            return null;
        }
        this._drawPickCanvas();
        const pickedColor = this.pickContext.getImageData(
            Math.round(screenX * resolutionMultiplier),
            Math.round(screenY * resolutionMultiplier),
            1,
            1
        ).data;
        uint32ToUint8[0] = pickedColor[0];
        uint32ToUint8[1] = pickedColor[1];
        uint32ToUint8[2] = pickedColor[2];
        uint32ToUint8[3] = 0;
        return this.getActorData(uint32[0]);
    }

    _drawHoveredActorLabel() {
        if (!this.hoveredActor || !this.hoveredActorLabel || !this.hoveredActor.canDraw() || !this.hoveredActorScreenPosition) {
            return;
        }
        if (this._isPlayerActor(this.hoveredActor) || this._isTargetActor(this.hoveredActor)) {
            return;
        }
        const ctx = this.mainContext;
        const canvas = this.mainCanvas;
        const paddingX = 10 * resolutionMultiplier;
        const paddingY = 6 * resolutionMultiplier;
        const fontSize = 13 * resolutionMultiplier;
        const lineHeight = fontSize + paddingY * 2;
        const margin = 14 * resolutionMultiplier;
        const screenX = this.hoveredActorScreenPosition.x * resolutionMultiplier;
        const screenY = this.hoveredActorScreenPosition.y * resolutionMultiplier;
        const maxWidth = Math.max(120 * resolutionMultiplier, canvas.width - margin * 2);
        const preferredX = screenX + margin;
        const preferredY = screenY - margin;

        ctx.save();
        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.font = "600 " + fontSize + "px Arial";
        ctx.textAlign = "left";
        ctx.textBaseline = "middle";

        const measuredWidth = ctx.measureText(this.hoveredActorLabel).width;
        const boxWidth = Math.min(measuredWidth + paddingX * 2, maxWidth);
        const maxX = canvas.width - margin - boxWidth;
        const minY = margin + lineHeight;
        const boxX = Math.max(margin, Math.min(maxX, preferredX));
        const boxY = Math.max(minY, preferredY);

        ctx.fillStyle = "rgba(15, 18, 23, 0.92)";
        ctx.strokeStyle = "rgba(220, 226, 235, 0.28)";
        ctx.lineWidth = resolutionMultiplier;
        ctx.fillRect(boxX, boxY - lineHeight, boxWidth, lineHeight);
        ctx.strokeRect(boxX, boxY - lineHeight, boxWidth, lineHeight);

        ctx.fillStyle = "#F4F6F9";
        ctx.fillText(this.hoveredActorLabel, boxX + paddingX, boxY - lineHeight / 2);
        ctx.restore();
    }

    _initMouseEvents() {
        var _this = this;
        var canvas = this.mainCanvas;
        var ctx = this.mainContext;
        var bgCtx = this.bgContext;

        canvas.addEventListener('mousedown', function (evt) {
            evt.preventDefault();
            _this.lastX = evt.offsetX || (evt.pageX - canvas.offsetLeft);
            _this.lastY = evt.offsetY || (evt.pageY - canvas.offsetTop);
            _this.mouseDown = {
                pt: ctx.transformedPoint(_this.lastX, _this.lastY),
                time: Date.now()
            }
            _this.dragged = false;
        }, false);

        canvas.addEventListener('mousemove', function (evt) {
            evt.preventDefault();
            _this.lastX = evt.offsetX || (evt.pageX - canvas.offsetLeft);
            _this.lastY = evt.offsetY || (evt.pageY - canvas.offsetTop);
            _this.dragged = true;
            if (_this.mouseDown) {
                var pt = ctx.transformedPoint(_this.lastX, _this.lastY);
                var downPt = _this.mouseDown.pt;
                ctx.translate(pt.x - downPt.x, pt.y - downPt.y);
                bgCtx.translate(pt.x - downPt.x, pt.y - downPt.y);
                _this.needBGUpdate = true;
                if (_this.animation === null) {
                    animateCanvas(noUpdateTime);
                }
                _this._clearHoveredActor();
            } else {
                const hoveredActor = _this._pickActorAtScreenPoint(_this.lastX, _this.lastY);
                _this._scheduleHoveredActor(hoveredActor, _this.lastX, _this.lastY);
            }
        }, false);

        canvas.addEventListener('mouseleave', function () {
            _this._clearHoveredActor();
        }, false);

        document.body.addEventListener('mouseup', function (evt) {
            if (_this.mouseDown && Date.now() - _this.mouseDown.time < 150) {
                const pickedActor = _this._pickActorAtScreenPoint(_this.lastX, _this.lastY);
                _this.selectActor(pickedActor ? pickedActor.id : 0, true);
            }
            _this.mouseDown = null;
        }, false);

        var zoom = function (evt) {
            evt.preventDefault();
            var delta = evt.wheelDelta ? evt.wheelDelta / 40 : evt.detail ? -evt.detail : 0;
            if (delta) {
                var pt = ctx.transformedPoint(_this.lastX, _this.lastY);
                ctx.translate(pt.x, pt.y);
                bgCtx.translate(pt.x, pt.y);
                var factor = Math.pow(1.1, delta);
                ctx.scale(factor, factor);
                if ((50 / (InchToPixel * _this.scale) < 10)) {            
                    ctx.scale( 1.0 / factor, 1.0 / factor);
                    factor = 1.0;
                }
                ctx.translate(-pt.x, -pt.y);
                bgCtx.scale(factor, factor);
                bgCtx.translate(-pt.x, -pt.y);
                _this.needBGUpdate = true;
                if (_this.animation === null) {
                    animateCanvas(noUpdateTime);
                }
            }
        };

        canvas.addEventListener('DOMMouseScroll', zoom, false);
        canvas.addEventListener('mousewheel', zoom, false);
    }

    _initTouchEvents() {
        // todo
    }

    setSpeed(value) {
        this.speed = value;
    }

    getSpeed() {
        if (this.backwards) {
            return -this.speed;
        }
        return this.speed;
    }

    toggleBackwards() {
        this.backwards = !this.backwards;
        return this.backwards;
    }

    toggleRange(index) {
        this.rangeControl[index].enabled = !this.rangeControl[index].enabled;
        animateCanvas(noUpdateTime);
    }

    setRangeRadius(index, value) {
        this.rangeControl[index].radius = value;
        animateCanvas(noUpdateTime);
    }

    // https://codepen.io/anon/pen/KrExzG
    _trackTransforms(ctx) {
        var svg = document.createElementNS("http://www.w3.org/2000/svg", 'svg');
        var xform = svg.createSVGMatrix();
        ctx.getTransform = function () {
            return xform;
        };

        var drawImage = ctx.drawImage;
        ctx.drawImage = function() {
            const image = arguments[0];
            if (!image || !image.complete || image.naturalWidth === 0) {
                return;
            }
            return drawImage.call(ctx, ...arguments);
        }

        var savedTransforms = [];
        var save = ctx.save;
        ctx.save = function () {
            savedTransforms.push(xform.translate(0, 0));
            return save.call(ctx);
        };

        var restore = ctx.restore;
        ctx.restore = function () {
            xform = savedTransforms.pop();
            return restore.call(ctx);
        };

        var scale = ctx.scale;
        var _this = this;
        ctx.scale = function (sx, sy) {
            xform = xform.scale(sx, sy);
            var xAxis = Math.sqrt(xform.a * xform.a + xform.b * xform.b);
            var yAxis = Math.sqrt(xform.c * xform.c + xform.d * xform.d);
            _this.scale = Math.max(xAxis, yAxis) / resolutionMultiplier;
            return scale.call(ctx, sx, sy);
        };
        

        var rotate = ctx.rotate;
        ctx.rotate = function (radians) {
            xform = xform.rotate(radians * 180 / Math.PI);
            return rotate.call(ctx, radians);
        };

        var translate = ctx.translate;
        ctx.translate = function (dx, dy) {
            xform = xform.translate(dx, dy);
            return translate.call(ctx, dx, dy);
        };

        var transform = ctx.transform;
        ctx.transform = function (a, b, c, d, e, f) {
            var m2 = svg.createSVGMatrix();
            m2.a = a;
            m2.b = b;
            m2.c = c;
            m2.d = d;
            m2.e = e;
            m2.f = f;
            xform = xform.multiply(m2);
            return transform.call(ctx, a, b, c, d, e, f);
        };

        var setTransform = ctx.setTransform;
        ctx.setTransform = function (a, b, c, d, e, f) {
            xform.a = a;
            xform.b = b;
            xform.c = c;
            xform.d = d;
            xform.e = e;
            xform.f = f;
            return setTransform.call(ctx, a, b, c, d, e, f);
        };

        var pt = svg.createSVGPoint();
        ctx.transformedPoint = function (x, y) {
            pt.x = x * resolutionMultiplier;
            pt.y = y * resolutionMultiplier;
            return pt.matrixTransform(xform.inverse());
        };
    }
    // animation
    _drawBGCanvas() {
        const _this = this;
        if (!this.needBGUpdate) {
            this.backgroundImages.forEach(x => {
                if (x.needsUpdate()) {
                    _this.needBGUpdate = true;
                }
            });
        }
        if (this.needBGUpdate || this._mustMoveToSelected()) {
            this.needBGUpdate = false;
            var ctx = this.bgContext;
            var canvas = this.bgCanvas;
            var p1 = ctx.transformedPoint(0, 0);
            var p2 = ctx.transformedPoint(canvas.width, canvas.height);
            ctx.clearRect(p1.x, p1.y, p2.x - p1.x, p2.y - p1.y);

            ctx.save();
            {
                ctx.setTransform(1, 0, 0, 1, 0, 0);
                ctx.clearRect(0, 0, canvas.width, canvas.height);
            }
            ctx.restore();

            //ctx.save();
            {

                this._moveToSelected(ctx);
                this.backgroundImages.draw(standardDraw);
                //ctx.globalCompositeOperation = "color-burn";
                ctx.save();
                {
                    ctx.setTransform(1, 0, 0, 1, 0, 0);
                    // draw scale
                    ctx.lineWidth = 3 * resolutionMultiplier;
                    ctx.strokeStyle = "#CC2200";
                    var pos = resolutionMultiplier * 70;
                    var width = resolutionMultiplier * 50;
                    var height = resolutionMultiplier * 6;
                    // main line
                    ctx.beginPath();
                    ctx.moveTo(pos, pos);
                    ctx.lineTo(pos + width, pos);
                    ctx.stroke();
                    ctx.lineWidth = 2 * resolutionMultiplier;
                    // right border
                    ctx.beginPath();
                    ctx.moveTo(pos - resolutionMultiplier, pos + height);
                    ctx.lineTo(pos - resolutionMultiplier, pos - height);
                    ctx.stroke();
                    // left border
                    ctx.beginPath();
                    ctx.moveTo(pos + width + resolutionMultiplier, pos + height);
                    ctx.lineTo(pos + width + resolutionMultiplier, pos - height);
                    ctx.stroke();
                    // text
                    var fontSize = 13 * resolutionMultiplier;
                    ctx.font = "bold " + fontSize + "px Comic Sans MS";
                    ctx.fillStyle = "#CC2200";
                    ctx.textAlign = "center";
                    ctx.fillText((50 / (InchToPixel * this.scale)).toFixed(1) + " units", resolutionMultiplier * 95, resolutionMultiplier * 60);
                }
                ctx.restore();
            }
            //ctx.restore();
            //ctx.globalCompositeOperation = 'normal';
        }
    }

    _drawActorOrientation(key) {
        if (this.actorOrientationData.has(key)) {
            this.actorOrientationData.get(key).draw();
        }
    }

    _drawPickCanvas() {
        var _this = this;
        var mainCtx = this.mainContext;
        var mainTransform = mainCtx.getTransform();
        var ctx = this.pickContext;
        var canvas = this.pickCanvas;
        var p1 = ctx.transformedPoint(0, 0);
        var p2 = ctx.transformedPoint(canvas.width, canvas.height);
        ctx.clearRect(p1.x, p1.y, p2.x - p1.x, p2.y - p1.y);
        ctx.save();
        {
            ctx.setTransform(1, 0, 0, 1, 0, 0);
            ctx.clearRect(0, 0, canvas.width, canvas.height);
        }
        ctx.restore();

        //ctx.save();
        {
            ctx.setTransform(mainTransform.a, mainTransform.b, mainTransform.c, mainTransform.d, mainTransform.e, mainTransform.f);


            if (!this.displaySettings.useActorHitboxWidth) {
                this.friendlyMobData.draw(selectablePickingDraw);
                this.friendlyPlayerData.draw(selectablePickingDraw);
                this.playerData.draw(selectablePickingDraw);
            }

            if (this.displaySettings.displayTrashMobs) {
                this.trashMobData.draw(selectablePickingDraw);
            }

            this.targetData.draw(selectablePickingDraw);
            this.targetPlayerData.draw(selectablePickingDraw);
            if (this.displaySettings.useActorHitboxWidth) {
                this.friendlyMobData.draw(selectablePickingDraw);
                this.friendlyPlayerData.draw(selectablePickingDraw);
                this.playerData.draw(selectablePickingDraw);
            }
            if (this.selectedActor !== null) {
                this.selectedActor.drawPicking();
            }
        }

        //ctx.restore();
    }

    _drawMainCanvas() {
        var _this = this;
        var ctx = this.mainContext;
        var canvas = this.mainCanvas;
        var p1 = ctx.transformedPoint(0, 0);
        var p2 = ctx.transformedPoint(canvas.width, canvas.height);
        ctx.clearRect(p1.x, p1.y, p2.x - p1.x, p2.y - p1.y);
        ctx.save();
        {
            ctx.setTransform(1, 0, 0, 1, 0, 0);
            ctx.clearRect(0, 0, canvas.width, canvas.height);
        }
        ctx.restore();
        //ctx.save();
        {

            this._moveToSelected(ctx);
            // Background items commonly overlap so they need to be drawn in the correct order by height
            // This is sorted in reverse order because the z axis is inverted
            animator.backgroundActorData.sort((x, y) => y.getHeight() - x.getHeight());
            for (let i = 0; i < animator.backgroundActorData.length; i++) {
                animator.backgroundActorData[i].draw();
            }
            if (this.displaySettings.displayMechanics) {
                this.mechanicActorData.draw(standardDraw);
            }

            if (this.displaySettings.displaySkillMechanics) {
                this.skillMechanicActorData.draw(standardDraw);
            }


            if (!this.displaySettings.useActorHitboxWidth) {
                this.friendlyMobData.draw(selectableDraw);
                this.friendlyPlayerData.draw(selectableDraw);
                this.playerData.draw(selectableDraw);
            }

            if (this.displaySettings.displayTrashMobs) {
                this.trashMobData.draw(selectableDraw);
            }

            this.targetData.draw(selectableDraw);
            this.targetPlayerData.draw(selectableDraw);
            if (this.displaySettings.useActorHitboxWidth) {
                this.friendlyMobData.draw(selectableDraw);
                this.friendlyPlayerData.draw(selectableDraw);
                this.playerData.draw(selectableDraw);
            }
            if (this.selectedActor !== null) {
                this.selectedActor.draw();
                this._drawActorOrientation(this.selectedActor.id);
            }
            if (this.displaySettings.displayMechanics) {
                this.overheadActorData.draw(standardDraw);
            }
            if (this.displaySettings.displaySquadMarkers) {
                this.squadMarkerData.draw(standardDraw);
                this.overheadSquadMarkerData.draw(standardDraw);
            }
            ctx.save();
            {
                ctx.setTransform(1, 0, 0, 1, 0, 0);
                // Screen space actors
                this.screenSpaceActorData.draw(standardDraw);
            }
            ctx.restore();
            this._drawHoveredActorLabel();
        }
        //ctx.restore();  
    }

    _mustMoveToSelected() {
        return this.displaySettings.followSelected && this.selectedActor !== null && this.selectedActor.canDraw();
    }

    _moveToSelected(ctx) {

        if (this._mustMoveToSelected()) {
            const pos = this.selectedActor.getPosition();
            if (pos !== null) {
                ctx.setTransform(1, 0, 0, 1, 0, 0);
                ctx.scale(this.scale * resolutionMultiplier, this.scale * resolutionMultiplier);
                const translateScale = 0.5 / resolutionMultiplier / this.scale
                ctx.translate(-pos.x + this.mainCanvas.width * translateScale, -pos.y + this.mainCanvas.height * translateScale);
            }
        }
    }
    draw() {
        if (!this.mainCanvas) {
            return;
        }    
        this._reselectIfEnglobed();
        //
        //this._drawPickCanvas();
        this._drawBGCanvas();
        this._drawMainCanvas();
        if (overheadAnimationFrame === maxOverheadAnimationFrame || overheadAnimationFrame === 0) {
            overheadAnimationIncrement *= -1;
        }
        overheadAnimationFrame += overheadAnimationIncrement;
    }
}

function animateCanvas(noRequest) {
    if (animator == null) {
        return;
    }
    let lastTime = animator.reactiveDataStatus.range.max;
    let firstTime = animator.reactiveDataStatus.range.min;
    if (noRequest > noUpdateTime && animator.animation !== null) {
        let curTime = new Date().getTime();
        let timeOffset = curTime - animator.prevTime;
        animator.prevTime = curTime;
        animator.reactiveDataStatus.time = Math.round(Math.max(Math.min(animator.reactiveDataStatus.time + animator.getSpeed() * timeOffset, lastTime), 0));
    }
    if ((animator.reactiveDataStatus.time === lastTime && !animator.backwards) || (animator.reactiveDataStatus.time === firstTime && animator.backwards)) {
        animator.stopAnimate(true);
    }
    animator.timeSlider.value = (animator.reactiveDataStatus.time - animator.reactiveDataStatus.range.min).toString()
    if (noRequest > updateText) {
        animator.updateTextInput();
    }
    animator.draw();
    if (noRequest > noUpdateTime && animator.animation !== null) {
        animator.animation = requestAnimationFrame(animateCanvas);
    }
}
/*
function initCombatReplay(actors, options) {
    // manipulation events
    canvas.addEventListener('touchstart', function (evt) {
        var touch = evt.changedTouches[0];
        if (!touch) {
            return;
        }
        lastX = (touch.pageX - canvas.offsetLeft);
        lastY = (touch.pageY - canvas.offsetTop);
        mouseDown = ctx.transformedPoint(lastX, lastY);
        dragged = false;
        return evt.preventDefault() && false;
    }, false);

    canvas.addEventListener('touchmove', function (evt) {
        var touch = evt.changedTouches[0];
        if (!touch) {
            return;
        }
        lastX = (touch.pageX - canvas.offsetLeft);
        lastY = (touch.pageY - canvas.offsetTop);
        dragged = true;
        if (mouseDown) {
            var pt = ctx.transformedPoint(lastX, lastY);
            ctx.translate(pt.x - mouseDown.x, pt.y - mouseDown.y);
            animateCanvas(noUpdateTime);
        }
        return evt.preventDefault() && false;
    }, false);
    document.body.addEventListener('touchend', function (evt) {
        mouseDown = null;
    }, false);
}
*/
