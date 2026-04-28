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
    selectedActorSource: null,
    hoveredActorKind: null,
    hoveredActorID: null,
    hoveredActorX: 0,
    hoveredActorY: 0,
    animated: false,
    viewRevision: 0,
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
            displayDamageOverlay: false,
            displayStickyDamageOverlayLinks: false,
            displayPositionOverlay: false,
            displayEnemyPositionOverlay: false,
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
        this.damageOverlayData = this._buildDamageOverlayData(options ? options.analysis : null);
        this.positionOverlayData = this._buildPositionOverlayData(options ? options.analysis : null);
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

    _bumpViewRevision() {
        this.reactiveDataStatus.viewRevision++;
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

    _setSelectedActor(actorId, source) {
        const currentActorId = this.reactiveDataStatus.selectedActorID;
        const currentValue = currentActorId == null ? null : String(currentActorId);
        const nextValue = actorId == null ? null : String(actorId);
        if (currentValue !== nextValue) {
            this.reactiveDataStatus.selectedActorSource = source || "external";
        }
        this.reactiveDataStatus.selectedActorID = actorId;
    }

    selectActor(actorId, keepIfEqual = false, source = "external") {
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
            this._setSelectedActor(null, source);
        } else {
            this.selectedActor = actor;
            this._setSelectedActor(actorId, source);
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
        this._setSelectedActor(actorId, "external");
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
        this._bumpViewRevision();
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
                this._setSelectedActor(this.selectedActor.id, "external");
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

    toggleDamageOverlay() {
        if (!this.hasDamageTakenOverlay()) {
            this.displaySettings.displayDamageOverlay = false;
            return false;
        }
        this.displaySettings.displayDamageOverlay = !this.displaySettings.displayDamageOverlay;
        animateCanvas(noUpdateTime);
        return this.displaySettings.displayDamageOverlay;
    }

    togglePositionOverlay() {
        if (!this.hasPositionOverlay()) {
            this.displaySettings.displayPositionOverlay = false;
            return false;
        }
        this.displaySettings.displayPositionOverlay = !this.displaySettings.displayPositionOverlay;
        animateCanvas(noUpdateTime);
        return this.displaySettings.displayPositionOverlay;
    }

    toggleEnemyPositionOverlay() {
        if (!this.hasEnemyPositionOverlay()) {
            this.displaySettings.displayEnemyPositionOverlay = false;
            return false;
        }
        this.displaySettings.displayEnemyPositionOverlay = !this.displaySettings.displayEnemyPositionOverlay;
        animateCanvas(noUpdateTime);
        return this.displaySettings.displayEnemyPositionOverlay;
    }

    setStickyDamageOverlayLinksEnabled(enabled) {
        this.displaySettings.displayStickyDamageOverlayLinks = !!enabled;
        if (this.animation === null) {
            animateCanvas(noUpdateTime);
        }
    }

    hasDamageTakenOverlay() {
        return !!(this.damageOverlayData &&
            this.damageOverlayData.entries &&
            this.damageOverlayData.entries.length > 0 &&
            this.damageOverlayData.scaleValue > 0);
    }

    hasPositionOverlay() {
        return !!(this.positionOverlayData &&
            this.positionOverlayData.hasCommander &&
            this.positionOverlayData.commanderId > 0 &&
            this.positionOverlayData.times &&
            this.positionOverlayData.times.length > 0 &&
            this.positionOverlayData.players &&
            Object.keys(this.positionOverlayData.players).length > 0);
    }

    hasEnemyPositionOverlay() {
        return this.hasPositionOverlay() && !!(logData && logData.targets && logData.targets.length > 0);
    }

    getDamageOverlayInfo(actorId) {
        if (!this.hasDamageTakenOverlay() || actorId == null) {
            return null;
        }
        const snapshotIndex = this._findDamageOverlaySnapshotIndex(this.reactiveDataStatus.time);
        if (snapshotIndex < 0) {
            return null;
        }
        const overlayData = this.damageOverlayData;
        for (let i = 0; i < overlayData.entries.length; i++) {
            const entry = overlayData.entries[i];
            if (String(entry.actorId) !== String(actorId)) {
                continue;
            }
            const damage = Number(entry.damageTaken[snapshotIndex] || 0);
            return {
                actorId: entry.actorId,
                targetSide: entry.targetSide,
                damage: damage,
                fullHeatDamage: overlayData.scaleValue,
                lookback: overlayData.lookback,
                heatPercent: Math.max(0, Math.min(100, damage * 100 / overlayData.scaleValue)),
                topContributors: this._getDamageOverlayContributorInfo(entry.topContributors ? entry.topContributors[snapshotIndex] : null),
            };
        }
        return null;
    }

    getActorScreenPosition(actorId) {
        if (!this.mainContext || actorId == null) {
            return null;
        }
        const actor = this.getActorData(actorId);
        if (!actor || !actor.canDraw()) {
            return null;
        }
        const position = actor.getPosition();
        if (position === null) {
            return null;
        }
        const transform = this.mainContext.getTransform();
        return {
            x: (position.x * transform.a + position.y * transform.c + transform.e) / resolutionMultiplier,
            y: (position.x * transform.b + position.y * transform.d + transform.f) / resolutionMultiplier,
        };
    }

    _getActorDisplayNameById(actorId) {
        const actor = this.getActorData(actorId);
        const actorLabel = this._getActorLabel(actor);
        if (actorLabel) {
            return actorLabel;
        }
        const collections = [logData.players, logData.targets, logData.enemies];
        for (let i = 0; i < collections.length; i++) {
            const collection = collections[i];
            if (!collection) {
                continue;
            }
            const match = collection.find(entry => entry && String(entry.uniqueID) === String(actorId));
            if (match && match.name) {
                return match.name;
            }
        }
        return "Actor " + actorId;
    }

    _getSkillDisplayName(skillId) {
        if (skillId != null && logData && logData.skillMap) {
            const skill = logData.skillMap["s" + skillId];
            if (skill && skill.name) {
                return skill.name;
            }
        }
        return skillId ? "Skill " + skillId : "Unknown skill";
    }

    _getDamageOverlayContributorInfo(contributors) {
        if (!Array.isArray(contributors) || contributors.length === 0) {
            return [];
        }
        const result = [];
        for (let i = 0; i < contributors.length; i++) {
            const contributor = contributors[i];
            if (!Array.isArray(contributor) || contributor.length < 3) {
                continue;
            }
            const sourceId = Number(contributor[0] || 0);
            const skillId = Number(contributor[1] || 0);
            const damage = Number(contributor[2] || 0);
            if (sourceId <= 0 || damage <= 0) {
                continue;
            }
            result.push({
                sourceId: sourceId,
                sourceName: this._getActorDisplayNameById(sourceId),
                skillId: skillId,
                skillName: this._getSkillDisplayName(skillId),
                damage: damage,
            });
        }
        return result;
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
        this._bumpViewRevision();
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
                _this._bumpViewRevision();
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
                _this.selectActor(pickedActor ? pickedActor.id : 0, true, "map");
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
                _this._bumpViewRevision();
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

    _buildDamageOverlayData(analysis) {
        if (!analysis || !analysis.times || analysis.times.length === 0) {
            return null;
        }
        if (analysis.damageOverlay && analysis.damageOverlay.entries && analysis.damageOverlay.entries.length > 0) {
            return {
                times: analysis.times,
                entries: analysis.damageOverlay.entries.map(entry => ({
                    actorId: entry.uniqueId,
                    targetSide: entry.targetSide,
                    damageTaken: entry.damageTaken,
                    topContributors: entry.topContributors || null,
                })),
                lookback: Number(analysis.damageOverlay.lookback || 1000),
                scaleValue: Math.max(Number(analysis.damageOverlay.fullHeatDamage || 25000), 1),
            };
        }

        const entries = [];
        const positiveValues = [];
        const addTargetTimelines = function (teamAnalysis, targetSide) {
            if (!teamAnalysis || !teamAnalysis.targets) {
                return;
            }
            const actorIds = Object.keys(teamAnalysis.targets);
            for (let i = 0; i < actorIds.length; i++) {
                const actorId = parseInt(actorIds[i], 10);
                const targetTimeline = teamAnalysis.targets[actorIds[i]];
                if (!targetTimeline || !targetTimeline.damageTaken || targetTimeline.damageTaken.length === 0) {
                    continue;
                }
                let hasDamage = false;
                for (let j = 0; j < targetTimeline.damageTaken.length; j++) {
                    const damage = Number(targetTimeline.damageTaken[j] || 0);
                    if (damage > 0) {
                        hasDamage = true;
                        positiveValues.push(damage);
                    }
                }
                if (hasDamage) {
                    entries.push({
                        actorId: actorId,
                        targetSide: targetSide,
                        damageTaken: targetTimeline.damageTaken,
                    });
                }
            }
        };

        addTargetTimelines(analysis.squad, "enemy");
        addTargetTimelines(analysis.enemy, "squad");
        if (entries.length === 0 || positiveValues.length === 0) {
            return null;
        }

        positiveValues.sort((left, right) => left - right);
        const percentileIndex = Math.min(
            positiveValues.length - 1,
            Math.max(0, Math.floor((positiveValues.length - 1) * 0.95))
        );
        const scaleValue = Math.max(positiveValues[percentileIndex], 1);
        return {
            times: analysis.times,
            entries: entries,
            lookback: Number(analysis.lookback || 3000),
            scaleValue: scaleValue,
        };
    }

    _buildPositionOverlayData(analysis) {
        if (!analysis || !analysis.times || analysis.times.length === 0 || !analysis.positioning) {
            return null;
        }
        const positioning = analysis.positioning;
        if (!positioning.hasCommander || !positioning.commanderId || !positioning.players) {
            return null;
        }
        return {
            times: analysis.times,
            hasCommander: !!positioning.hasCommander,
            commanderId: Number(positioning.commanderId || 0),
            desiredCommanderDistance: Number(positioning.desiredCommanderDistance || 240),
            mingledCommanderDistance: Number(positioning.mingledCommanderDistance || 180),
            ignoreCommanderDistance: Number(positioning.ignoreCommanderDistance || 3000),
            engageRange: Number(positioning.engageRange || 1200),
            mingledRange: Number(positioning.mingledRange || 100),
            mingled: positioning.mingled || [],
            engagedEnemyCount: positioning.engagedEnemyCount || [],
            eligiblePlayerCount: positioning.eligiblePlayerCount || [],
            players: positioning.players || {},
        };
    }

    _findSnapshotIndex(times, time) {
        if (!times || times.length === 0) {
            return -1;
        }
        let low = 0;
        let high = times.length - 1;
        while (low < high) {
            const mid = Math.floor((low + high) / 2);
            if (times[mid] < time) {
                low = mid + 1;
            } else {
                high = mid;
            }
        }
        if (low === 0) {
            return 0;
        }
        return Math.abs(times[low] - time) < Math.abs(times[low - 1] - time) ? low : low - 1;
    }

    _findDamageOverlaySnapshotIndex(time) {
        const times = this.damageOverlayData ? this.damageOverlayData.times : null;
        return this._findSnapshotIndex(times, time);
    }

    _findPositionOverlaySnapshotIndex(time) {
        const times = this.positionOverlayData ? this.positionOverlayData.times : null;
        return this._findSnapshotIndex(times, time);
    }

    _drawDamageOverlayPulse(ctx, position, actorSize, damage, scaleValue, targetSide) {
        const normalized = Math.max(0.0, Math.min(1.0, damage / scaleValue));
        if (normalized <= 0) {
            return;
        }
        const eased = Math.pow(normalized, 0.42);
        const baseRadius = Math.max(actorSize * 0.82, 13 / this.scale);
        const radius = baseRadius * (1.08 + eased * 1.14);
        const innerRadius = Math.max(radius * 0.2, 1 / this.scale);
        const alpha = Math.min(0.66, 0.14 + eased * 0.5);
        const useCoolHeat = targetSide === "enemy";
        const edgeRed = useCoolHeat ? Math.round(52 - normalized * 34) : 255;
        const edgeGreen = useCoolHeat ? 255 : Math.round(208 - normalized * 146);
        const edgeBlue = useCoolHeat ? Math.round(225 - normalized * 120) : Math.round(72 - normalized * 36);
        const coreRed = useCoolHeat ? Math.round(130 - normalized * 70) : 255;
        const coreGreen = useCoolHeat ? 255 : Math.round(236 - normalized * 58);
        const coreBlue = useCoolHeat ? Math.round(245 - normalized * 95) : Math.round(156 - normalized * 110);
        const gradient = ctx.createRadialGradient(
            position.x,
            position.y,
            innerRadius,
            position.x,
            position.y,
            radius
        );
        gradient.addColorStop(0, "rgba(" + coreRed + ", " + coreGreen + ", " + coreBlue + ", " + Math.min(0.76, alpha + 0.1).toFixed(3) + ")");
        gradient.addColorStop(0.48, "rgba(" + edgeRed + ", " + edgeGreen + ", " + edgeBlue + ", " + alpha.toFixed(3) + ")");
        gradient.addColorStop(1, "rgba(" + edgeRed + ", " + edgeGreen + ", " + edgeBlue + ", 0)");

        ctx.save();
        ctx.beginPath();
        ctx.arc(position.x, position.y, radius, 0, 2 * Math.PI);
        ctx.fillStyle = gradient;
        ctx.fill();
        ctx.lineWidth = Math.max(1.2 / this.scale, radius * 0.05);
        ctx.strokeStyle = "rgba(" + edgeRed + ", " + edgeGreen + ", " + edgeBlue + ", " + Math.min(0.62, alpha + 0.05).toFixed(3) + ")";
        ctx.stroke();
        ctx.restore();
    }

    _drawDamageTakenOverlay() {
        if (!this.displaySettings.displayDamageOverlay || !this.hasDamageTakenOverlay()) {
            return;
        }
        const snapshotIndex = this._findDamageOverlaySnapshotIndex(this.reactiveDataStatus.time);
        if (snapshotIndex < 0) {
            return;
        }
        const ctx = this.mainContext;
        const overlayData = this.damageOverlayData;
        for (let i = 0; i < overlayData.entries.length; i++) {
            const entry = overlayData.entries[i];
            const damage = Number(entry.damageTaken[snapshotIndex] || 0);
            if (damage <= 0) {
                continue;
            }
            const actor = this.getSelectableActorData(entry.actorId);
            if (!actor || !actor.canDraw()) {
                continue;
            }
            const position = actor.getPosition();
            if (position === null) {
                continue;
            }
            this._drawDamageOverlayPulse(ctx, position, actor.getSize(), damage, overlayData.scaleValue, entry.targetSide);
        }
    }

    _drawDamageContributorLink(ctx, sourcePosition, targetPosition, contributor, targetSide, maxContributorDamage, index) {
        const dx = targetPosition.x - sourcePosition.x;
        const dy = targetPosition.y - sourcePosition.y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        if (distance <= 0) {
            return;
        }
        const share = maxContributorDamage > 0 ? Math.max(0.0, Math.min(1.0, contributor.damage / maxContributorDamage)) : 1.0;
        const useCoolHeat = targetSide === "enemy";
        const red = useCoolHeat ? 84 : 255;
        const green = useCoolHeat ? 255 : 126;
        const blue = useCoolHeat ? 216 : 68;
        const alpha = 0.32 + share * 0.38;
        const width = (1.4 + share * 2.0) / this.scale;
        const bend = (index - 1) * 14 / this.scale;
        const normalX = -dy / distance;
        const normalY = dx / distance;
        const controlX = (sourcePosition.x + targetPosition.x) * 0.5 + normalX * bend;
        const controlY = (sourcePosition.y + targetPosition.y) * 0.5 + normalY * bend;

        ctx.save();
        ctx.lineCap = "round";
        ctx.lineJoin = "round";
        ctx.beginPath();
        ctx.moveTo(sourcePosition.x, sourcePosition.y);
        ctx.quadraticCurveTo(controlX, controlY, targetPosition.x, targetPosition.y);
        ctx.strokeStyle = "rgba(" + red + ", " + green + ", " + blue + ", " + (alpha * 0.28).toFixed(3) + ")";
        ctx.lineWidth = width + 5 / this.scale;
        ctx.stroke();

        ctx.beginPath();
        ctx.moveTo(sourcePosition.x, sourcePosition.y);
        ctx.quadraticCurveTo(controlX, controlY, targetPosition.x, targetPosition.y);
        ctx.strokeStyle = "rgba(" + red + ", " + green + ", " + blue + ", " + alpha.toFixed(3) + ")";
        ctx.lineWidth = width;
        ctx.stroke();

        ctx.beginPath();
        ctx.arc(sourcePosition.x, sourcePosition.y, Math.max(3.5 / this.scale, width * 1.4), 0, 2 * Math.PI);
        ctx.fillStyle = "rgba(" + red + ", " + green + ", " + blue + ", " + Math.min(0.85, alpha + 0.1).toFixed(3) + ")";
        ctx.fill();
        ctx.restore();
    }

    _drawSelectedDamageContributorLinks() {
        if (!this.displaySettings.displayStickyDamageOverlayLinks || !this.displaySettings.displayDamageOverlay || !this.hasDamageTakenOverlay()) {
            return;
        }
        const actorId = this.reactiveDataStatus.selectedActorID;
        if (actorId == null) {
            return;
        }
        const info = this.getDamageOverlayInfo(actorId);
        if (!info || !info.topContributors || info.topContributors.length === 0) {
            return;
        }
        const targetActor = this.getActorData(info.actorId);
        if (!targetActor || !targetActor.canDraw()) {
            return;
        }
        const targetPosition = targetActor.getPosition();
        if (targetPosition === null) {
            return;
        }
        const maxContributorDamage = Math.max(...info.topContributors.map(contributor => contributor.damage));
        for (let i = 0; i < info.topContributors.length; i++) {
            const contributor = info.topContributors[i];
            const sourceActor = this.getActorData(contributor.sourceId);
            if (!sourceActor || !sourceActor.canDraw()) {
                continue;
            }
            const sourcePosition = sourceActor.getPosition();
            if (sourcePosition === null) {
                continue;
            }
            this._drawDamageContributorLink(this.mainContext, sourcePosition, targetPosition, contributor, info.targetSide, maxContributorDamage, i);
        }
    }

    _getPositionOverlaySnapshot() {
        if (!this.displaySettings.displayPositionOverlay || !this.hasPositionOverlay()) {
            return null;
        }
        const snapshotIndex = this._findPositionOverlaySnapshotIndex(this.reactiveDataStatus.time);
        if (snapshotIndex < 0) {
            return null;
        }
        const overlayData = this.positionOverlayData;
        const commanderActor = this.getActorData(overlayData.commanderId);
        if (!commanderActor || !commanderActor.canDraw()) {
            return null;
        }
        const commanderPosition = commanderActor.getPosition();
        if (commanderPosition === null) {
            return null;
        }
        const mingled = !!overlayData.mingled[snapshotIndex];
        return {
            overlayData: overlayData,
            snapshotIndex: snapshotIndex,
            commanderActor: commanderActor,
            commanderPosition: commanderPosition,
            mingled: mingled,
            stackDistance: mingled ? overlayData.mingledCommanderDistance : overlayData.desiredCommanderDistance,
            engagedEnemyCount: Number(overlayData.engagedEnemyCount[snapshotIndex] || 0),
            eligiblePlayerCount: Number(overlayData.eligiblePlayerCount[snapshotIndex] || 0),
        };
    }

    _drawPositionOverlayRing(ctx, position, radiusUnits, strokeStyle, fillStyle, lineWidth, dash) {
        if (radiusUnits <= 0) {
            return;
        }
        ctx.save();
        ctx.beginPath();
        ctx.arc(position.x, position.y, InchToPixel * radiusUnits, 0, 2 * Math.PI);
        if (dash && dash.length > 0) {
            ctx.setLineDash(dash.map(value => value / this.scale));
        }
        if (fillStyle) {
            ctx.fillStyle = fillStyle;
            ctx.fill();
        }
        ctx.lineWidth = lineWidth / this.scale;
        ctx.strokeStyle = strokeStyle;
        ctx.stroke();
        ctx.restore();
    }

    _drawPositionOverlayRules() {
        const snapshot = this._getPositionOverlaySnapshot();
        if (!snapshot) {
            return;
        }
        const ctx = this.mainContext;
        const overlayData = snapshot.overlayData;
        const commanderPosition = snapshot.commanderPosition;
        const stackStroke = snapshot.mingled ? "rgba(255, 216, 92, 0.72)" : "rgba(111, 255, 166, 0.62)";
        const stackFill = snapshot.mingled ? "rgba(255, 216, 92, 0.075)" : "rgba(111, 255, 166, 0.06)";
        const mingledStroke = snapshot.mingled ? "rgba(255, 116, 234, 0.78)" : "rgba(126, 232, 255, 0.25)";

        this._drawPositionOverlayRing(ctx, commanderPosition, overlayData.engageRange, "rgba(116, 188, 255, 0.28)", null, 1.4, [9, 7]);
        this._drawPositionOverlayRing(ctx, commanderPosition, snapshot.stackDistance, stackStroke, stackFill, 2.2, []);
        this._drawPositionOverlayRing(ctx, commanderPosition, overlayData.mingledRange, mingledStroke, snapshot.mingled ? "rgba(255, 116, 234, 0.08)" : null, snapshot.mingled ? 2.0 : 1.2, [4, 4]);
    }

    _getMedian(values) {
        if (!values || values.length === 0) {
            return 0;
        }
        const sorted = [...values].sort((left, right) => left - right);
        const middle = Math.floor(sorted.length / 2);
        if (sorted.length % 2 === 1) {
            return sorted[middle];
        }
        return (sorted[middle - 1] + sorted[middle]) / 2;
    }

    _getEnemyPositionOverlayState() {
        if (!this.displaySettings.displayEnemyPositionOverlay || !this.hasEnemyPositionOverlay()) {
            return null;
        }
        const snapshot = this._getPositionOverlaySnapshot();
        if (!snapshot || snapshot.engagedEnemyCount <= 0) {
            return null;
        }
        const enemyPositions = [];
        for (let i = 0; i < logData.targets.length; i++) {
            const target = logData.targets[i];
            if (!target || target.uniqueID == null) {
                continue;
            }
            const actor = this.getActorData(target.uniqueID);
            if (!actor || !actor.canDraw()) {
                continue;
            }
            const position = actor.getPosition();
            if (position === null) {
                continue;
            }
            if (!this._isPositionWithinRange(position, snapshot.commanderPosition, snapshot.overlayData.engageRange)) {
                continue;
            }
            enemyPositions.push(position);
        }
        if (enemyPositions.length === 0) {
            return null;
        }
        return {
            center: {
                x: this._getMedian(enemyPositions.map(position => position.x)),
                y: this._getMedian(enemyPositions.map(position => position.y)),
            },
            count: enemyPositions.length,
            radius: snapshot.overlayData.desiredCommanderDistance,
        };
    }

    _isPositionWithinRange(left, right, rangeUnits) {
        const range = InchToPixel * rangeUnits;
        const dx = left.x - right.x;
        const dy = left.y - right.y;
        return dx * dx + dy * dy <= range * range;
    }

    _drawEnemyPositionOverlay() {
        const state = this._getEnemyPositionOverlayState();
        if (!state) {
            return;
        }
        const ctx = this.mainContext;
        const center = state.center;
        this._drawPositionOverlayRing(ctx, center, state.radius, "rgba(255, 92, 92, 0.72)", "rgba(255, 92, 92, 0.065)", 2.2, []);

        ctx.save();
        ctx.lineCap = "round";
        ctx.lineWidth = 2.2 / this.scale;
        ctx.strokeStyle = "rgba(255, 92, 92, 0.88)";
        ctx.fillStyle = "rgba(255, 92, 92, 0.72)";
        const markerRadius = 6 / this.scale;
        const markerArm = 12 / this.scale;
        ctx.beginPath();
        ctx.arc(center.x, center.y, markerRadius, 0, 2 * Math.PI);
        ctx.fill();
        ctx.beginPath();
        ctx.moveTo(center.x - markerArm, center.y);
        ctx.lineTo(center.x + markerArm, center.y);
        ctx.moveTo(center.x, center.y - markerArm);
        ctx.lineTo(center.x, center.y + markerArm);
        ctx.stroke();
        ctx.restore();
    }

    _getPositionOverlayPlayerState(timeline, snapshotIndex) {
        if (!timeline || !timeline.eligible || !timeline.eligible[snapshotIndex]) {
            return null;
        }
        const reasons = [];
        if (timeline.overextended && timeline.overextended[snapshotIndex]) {
            reasons.push("overextended");
        }
        if (timeline.lateralRisk && timeline.lateralRisk[snapshotIndex]) {
            reasons.push("lateralRisk");
        }
        if (timeline.tooFar && timeline.tooFar[snapshotIndex]) {
            reasons.push("tooFar");
        }
        return {
            inPosition: !!(timeline.inPosition && timeline.inPosition[snapshotIndex]),
            reasons: reasons,
        };
    }

    getPositionOverlayInfo(actorId) {
        if (!this.displaySettings.displayPositionOverlay || actorId == null) {
            return null;
        }
        const snapshot = this._getPositionOverlaySnapshot();
        if (!snapshot || snapshot.engagedEnemyCount <= 0) {
            return null;
        }
        if (String(actorId) === String(snapshot.overlayData.commanderId)) {
            return null;
        }
        const timeline = snapshot.overlayData.players[String(actorId)] || snapshot.overlayData.players[actorId];
        const state = this._getPositionOverlayPlayerState(timeline, snapshot.snapshotIndex);
        if (!state || state.inPosition || state.reasons.length === 0) {
            return null;
        }
        const distance = timeline.distanceToCommander ? Number(timeline.distanceToCommander[snapshot.snapshotIndex] || 0) : 0;
        const enemiesCloser = timeline.enemiesCloserThanCommander ? Number(timeline.enemiesCloserThanCommander[snapshot.snapshotIndex] || 0) : 0;
        const enemiesAhead = timeline.enemiesAheadOfCommander ? Number(timeline.enemiesAheadOfCommander[snapshot.snapshotIndex] || 0) : 0;
        return {
            actorId: Number(actorId),
            reasons: state.reasons,
            distanceToCommander: distance,
            enemiesCloserThanCommander: enemiesCloser,
            enemiesAheadOfCommander: enemiesAhead,
            stackDistance: snapshot.stackDistance,
            mingled: snapshot.mingled,
        };
    }

    _getPositionOverlayReasonColor(reason) {
        switch (reason) {
            case "overextended":
                return { red: 255, green: 74, blue: 74 };
            case "lateralRisk":
                return { red: 236, green: 92, blue: 255 };
            case "tooFar":
                return { red: 255, green: 184, blue: 74 };
            default:
                return { red: 106, green: 255, blue: 152 };
        }
    }

    _drawPositionPlayerHalo(ctx, position, radius, color, lineWidth, strokeAlpha, fillAlpha, dash) {
        ctx.save();
        ctx.beginPath();
        ctx.arc(position.x, position.y, radius, 0, 2 * Math.PI);
        if (dash && dash.length > 0) {
            ctx.setLineDash(dash.map(value => value / this.scale));
        }
        if (fillAlpha > 0) {
            ctx.fillStyle = "rgba(" + color.red + ", " + color.green + ", " + color.blue + ", " + fillAlpha.toFixed(3) + ")";
            ctx.fill();
        }
        ctx.lineWidth = lineWidth / this.scale;
        ctx.strokeStyle = "rgba(" + color.red + ", " + color.green + ", " + color.blue + ", " + strokeAlpha.toFixed(3) + ")";
        ctx.stroke();
        ctx.restore();
    }

    _drawPositionOverlayPlayerHighlights() {
        const snapshot = this._getPositionOverlaySnapshot();
        if (!snapshot || snapshot.engagedEnemyCount <= 0 || snapshot.eligiblePlayerCount <= 0) {
            return;
        }
        const ctx = this.mainContext;
        const playerIds = Object.keys(snapshot.overlayData.players);
        for (let i = 0; i < playerIds.length; i++) {
            const playerId = Number(playerIds[i]);
            if (playerId === snapshot.overlayData.commanderId) {
                continue;
            }
            const state = this._getPositionOverlayPlayerState(snapshot.overlayData.players[playerIds[i]], snapshot.snapshotIndex);
            if (!state) {
                continue;
            }
            const actor = this.getActorData(playerId);
            if (!actor || !actor.canDraw()) {
                continue;
            }
            const position = actor.getPosition();
            if (position === null) {
                continue;
            }
            const baseRadius = Math.max(actor.getSize() * 0.72, 10 / this.scale);
            if (state.inPosition) {
                this._drawPositionPlayerHalo(ctx, position, baseRadius, this._getPositionOverlayReasonColor("inPosition"), 1.4, 0.46, 0.0, []);
                continue;
            }
            const primaryReason = state.reasons[0] || "tooFar";
            const primaryColor = this._getPositionOverlayReasonColor(primaryReason);
            this._drawPositionPlayerHalo(ctx, position, baseRadius + 2 / this.scale, primaryColor, 3.2, 0.96, 0.16, []);
            for (let reasonIndex = 1; reasonIndex < state.reasons.length; reasonIndex++) {
                const color = this._getPositionOverlayReasonColor(state.reasons[reasonIndex]);
                this._drawPositionPlayerHalo(ctx, position, baseRadius + (5 + reasonIndex * 3) / this.scale, color, 1.7, 0.78, 0.0, [4, 3]);
            }
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

            this._drawDamageTakenOverlay();
            this._drawPositionOverlayRules();
            this._drawEnemyPositionOverlay();

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
            this._drawPositionOverlayPlayerHighlights();
            this._drawSelectedDamageContributorLinks();
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
