/* SlickGrid v2.3.2 2016-08-21
 * This file was built by copying the individual files from the GitHub repo.
 * There are no modifications/customizations. * 
 */

//#region jquery.event.drag

/*!
 * jquery.event.drag - v 2.3.0
 * Copyright (c) 2010 Three Dub Media - http://threedubmedia.com
 * Open Source MIT License - http://threedubmedia.com/code/license
 */
// Created: 2008-06-04
// Updated: 2012-05-21
// Updated: 2016-08-16   Luiz Gonzaga dos Santos Filho
// REQUIRES: jquery 1.8 +, , event.drag 2.3.0
// TESTED WITH: jQuery 1.8.3, 1.11.2, 2.2.4, and 3.1.0

; (function ($) {
    // add the jquery instance method
    $.fn.drag = function (str, arg, opts) {
        // figure out the event type
        var type = typeof str == "string" ? str : "",
            // figure out the event handler...
            fn = $.isFunction(str) ? str : $.isFunction(arg) ? arg : null;
        // fix the event type
        if (type.indexOf("drag") !== 0)
            type = "drag" + type;
        // were options passed
        opts = (str == fn ? arg : opts) || {};
        // trigger or bind event handler
        return fn ? this.on(type, opts, fn) : this.trigger(type);
    };

    // local refs (increase compression)
    var $event = $.event,
        $special = $event.special,
        // configure the drag special event
        drag = $special.drag = {

            // these are the default settings
            defaults: {
                which: 1, // mouse button pressed to start drag sequence
                distance: 0, // distance dragged before dragstart
                not: ':input', // selector to suppress dragging on target elements
                handle: null, // selector to match handle target elements
                relative: false, // true to use "position", false to use "offset"
                drop: true, // false to suppress drop events, true or selector to allow
                click: false // false to suppress click events after dragend (no proxy)
            },

            // the key name for stored drag data
            datakey: "dragdata",

            // prevent bubbling for better performance
            noBubble: true,

            // count bound related events
            add: function (obj) {
                // read the interaction data
                var data = $.data(this, drag.datakey),
                    // read any passed options
                    opts = obj.data || {};
                // count another realted event
                data.related += 1;
                // extend data options bound with this event
                // don't iterate "opts" in case it is a node
                $.each(drag.defaults, function (key, def) {
                    if (opts[key] !== undefined)
                        data[key] = opts[key];
                });
            },

            // forget unbound related events
            remove: function () {
                $.data(this, drag.datakey).related -= 1;
            },

            // configure interaction, capture settings
            setup: function () {
                // check for related events
                if ($.data(this, drag.datakey))
                    return;
                // initialize the drag data with copied defaults
                var data = $.extend({ related: 0 }, drag.defaults);
                // store the interaction data
                $.data(this, drag.datakey, data);
                // bind the mousedown event, which starts drag interactions
                $event.add(this, "touchstart mousedown", drag.init, data);
                // prevent image dragging in IE...
                if (this.attachEvent)
                    this.attachEvent("ondragstart", drag.dontstart);
            },

            // destroy configured interaction
            teardown: function () {
                var data = $.data(this, drag.datakey) || {};
                // check for related events
                if (data.related)
                    return;
                // remove the stored data
                $.removeData(this, drag.datakey);
                // remove the mousedown event
                $event.remove(this, "touchstart mousedown", drag.init);
                // enable text selection
                drag.textselect(true);
                // un-prevent image dragging in IE...
                if (this.detachEvent)
                    this.detachEvent("ondragstart", drag.dontstart);
            },

            // initialize the interaction
            init: function (event) {
                // sorry, only one touch at a time
                if (drag.touched)
                    return;
                // the drag/drop interaction data
                var dd = event.data, results;
                // check the which directive
                if (event.which != 0 && dd.which > 0 && event.which != dd.which)
                    return;
                // check for suppressed selector
                if ($(event.target).is(dd.not))
                    return;
                // check for handle selector
                if (dd.handle && !$(event.target).closest(dd.handle, event.currentTarget).length)
                    return;

                drag.touched = event.type == 'touchstart' ? this : null;
                dd.propagates = 1;
                dd.mousedown = this;
                dd.interactions = [drag.interaction(this, dd)];
                dd.target = event.target;
                dd.pageX = event.pageX;
                dd.pageY = event.pageY;
                dd.dragging = null;
                // handle draginit event...
                results = drag.hijack(event, "draginit", dd);
                // early cancel
                if (!dd.propagates)
                    return;
                // flatten the result set
                results = drag.flatten(results);
                // insert new interaction elements
                if (results && results.length) {
                    dd.interactions = [];
                    $.each(results, function () {
                        dd.interactions.push(drag.interaction(this, dd));
                    });
                }
                // remember how many interactions are propagating
                dd.propagates = dd.interactions.length;
                // locate and init the drop targets
                if (dd.drop !== false && $special.drop)
                    $special.drop.handler(event, dd);
                // disable text selection
                drag.textselect(false);
                // bind additional events...
                if (drag.touched)
                    $event.add(drag.touched, "touchmove touchend", drag.handler, dd);
                else
                    $event.add(document, "mousemove mouseup", drag.handler, dd);
                // helps prevent text selection or scrolling
                if (!drag.touched || dd.live)
                    return false;
            },

            // returns an interaction object
            interaction: function (elem, dd) {
                var offset = $(elem)[dd.relative ? "position" : "offset"]() || { top: 0, left: 0 };
                return {
                    drag: elem,
                    callback: new drag.callback(),
                    droppable: [],
                    offset: offset
                };
            },

            // handle drag-releatd DOM events
            handler: function (event) {
                // read the data before hijacking anything
                var dd = event.data;
                // handle various events
                switch (event.type) {
                    // mousemove, check distance, start dragging
                    case !dd.dragging && 'touchmove':
                        event.preventDefault();
                    case !dd.dragging && 'mousemove':
                        //  drag tolerance, x² + y² = distance²
                        if (Math.pow(event.pageX - dd.pageX, 2) + Math.pow(event.pageY - dd.pageY, 2) < Math.pow(dd.distance, 2))
                            break; // distance tolerance not reached
                        event.target = dd.target; // force target from "mousedown" event (fix distance issue)
                        drag.hijack(event, "dragstart", dd); // trigger "dragstart"
                        if (dd.propagates) // "dragstart" not rejected
                            dd.dragging = true; // activate interaction
                        // mousemove, dragging
                    case 'touchmove':
                        event.preventDefault();
                    case 'mousemove':
                        if (dd.dragging) {
                            // trigger "drag"
                            drag.hijack(event, "drag", dd);
                            if (dd.propagates) {
                                // manage drop events
                                if (dd.drop !== false && $special.drop)
                                    $special.drop.handler(event, dd); // "dropstart", "dropend"
                                break; // "drag" not rejected, stop
                            }
                            event.type = "mouseup"; // helps "drop" handler behave
                        }
                        // mouseup, stop dragging
                    case 'touchend':
                    case 'mouseup':
                    default:
                        if (drag.touched)
                            $event.remove(drag.touched, "touchmove touchend", drag.handler); // remove touch events
                        else
                            $event.remove(document, "mousemove mouseup", drag.handler); // remove page events
                        if (dd.dragging) {
                            if (dd.drop !== false && $special.drop)
                                $special.drop.handler(event, dd); // "drop"
                            drag.hijack(event, "dragend", dd); // trigger "dragend"
                        }
                        drag.textselect(true); // enable text selection
                        // if suppressing click events...
                        if (dd.click === false && dd.dragging)
                            $.data(dd.mousedown, "suppress.click", new Date().getTime() + 5);
                        dd.dragging = drag.touched = false; // deactivate element
                        break;
                }
            },

            // re-use event object for custom events
            hijack: function (event, type, dd, x, elem) {
                // not configured
                if (!dd)
                    return;
                // remember the original event and type
                var orig = { event: event.originalEvent, type: event.type },
                    // is the event drag related or drog related?
                    mode = type.indexOf("drop") ? "drag" : "drop",
                    // iteration vars
                    result, i = x || 0, ia, $elems, callback,
                    len = !isNaN(x) ? x : dd.interactions.length;
                // modify the event type
                event.type = type;
                // protects originalEvent from side-effects
                var noop = function () { };
                event.originalEvent = new jQuery.Event(orig.event, {
                    preventDefault: noop,
                    stopPropagation: noop,
                    stopImmediatePropagation: noop
                });
                // initialize the results
                dd.results = [];
                // handle each interacted element
                do if (ia = dd.interactions[i]) {
                    // validate the interaction
                    if (type !== "dragend" && ia.cancelled)
                        continue;
                    // set the dragdrop properties on the event object
                    callback = drag.properties(event, dd, ia);
                    // prepare for more results
                    ia.results = [];
                    // handle each element
                    $(elem || ia[mode] || dd.droppable).each(function (p, subject) {
                        // identify drag or drop targets individually
                        callback.target = subject;
                        // force propagtion of the custom event
                        event.isPropagationStopped = function () { return false; };
                        // handle the event
                        result = subject ? $event.dispatch.call(subject, event, callback) : null;
                        // stop the drag interaction for this element
                        if (result === false) {
                            if (mode == "drag") {
                                ia.cancelled = true;
                                dd.propagates -= 1;
                            }
                            if (type == "drop") {
                                ia[mode][p] = null;
                            }
                        }
                            // assign any dropinit elements
                        else if (type == "dropinit")
                            ia.droppable.push(drag.element(result) || subject);
                        // accept a returned proxy element
                        if (type == "dragstart")
                            ia.proxy = $(drag.element(result) || ia.drag)[0];
                        // remember this result
                        ia.results.push(result);
                        // forget the event result, for recycling
                        delete event.result;
                        // break on cancelled handler
                        if (type !== "dropinit")
                            return result;
                    });
                    // flatten the results
                    dd.results[i] = drag.flatten(ia.results);
                    // accept a set of valid drop targets
                    if (type == "dropinit")
                        ia.droppable = drag.flatten(ia.droppable);
                    // locate drop targets
                    if (type == "dragstart" && !ia.cancelled)
                        callback.update();
                }
                while (++i < len)
                // restore the original event & type
                event.type = orig.type;
                event.originalEvent = orig.event;
                // return all handler results
                return drag.flatten(dd.results);
            },

            // extend the callback object with drag/drop properties...
            properties: function (event, dd, ia) {
                var obj = ia.callback;
                // elements
                obj.drag = ia.drag;
                obj.proxy = ia.proxy || ia.drag;
                // starting mouse position
                obj.startX = dd.pageX;
                obj.startY = dd.pageY;
                // current distance dragged
                obj.deltaX = event.pageX - dd.pageX;
                obj.deltaY = event.pageY - dd.pageY;
                // original element position
                obj.originalX = ia.offset.left;
                obj.originalY = ia.offset.top;
                // adjusted element position
                obj.offsetX = obj.originalX + obj.deltaX;
                obj.offsetY = obj.originalY + obj.deltaY;
                // assign the drop targets information
                obj.drop = drag.flatten((ia.drop || []).slice());
                obj.available = drag.flatten((ia.droppable || []).slice());
                return obj;
            },

            // determine is the argument is an element or jquery instance
            element: function (arg) {
                if (arg && (arg.jquery || arg.nodeType == 1))
                    return arg;
            },

            // flatten nested jquery objects and arrays into a single dimension array
            flatten: function (arr) {
                return $.map(arr, function (member) {
                    return member && member.jquery ? $.makeArray(member) :
                        member && member.length ? drag.flatten(member) : member;
                });
            },

            // toggles text selection attributes ON (true) or OFF (false)
            textselect: function (bool) {
                $(document)[bool ? "off" : "on"]("selectstart", drag.dontstart)
                    .css("MozUserSelect", bool ? "" : "none");
                // .attr("unselectable", bool ? "off" : "on" )
                document.unselectable = bool ? "off" : "on";
            },

            // suppress "selectstart" and "ondragstart" events
            dontstart: function () {
                return false;
            },

            // a callback instance contructor
            callback: function () { }

        };

    // callback methods
    drag.callback.prototype = {
        update: function () {
            if ($special.drop && this.available.length)
                $.each(this.available, function (i) {
                    $special.drop.locate(this, i);
                });
        }
    };

    // patch $.event.$dispatch to allow suppressing clicks
    var $dispatch = $event.dispatch;
    $event.dispatch = function (event) {
        if ($.data(this, "suppress." + event.type) - new Date().getTime() > 0) {
            $.removeData(this, "suppress." + event.type);
            return;
        }
        return $dispatch.apply(this, arguments);
    };

    // share the same special event configuration with related events...
    $special.draginit = $special.dragstart = $special.dragend = drag;
})(jQuery);

//#endregion End jquery.event.drag

//#region jquery.event.drop

/*!
 * jquery.event.drop - v 2.3.0
 * Copyright (c) 2010 Three Dub Media - http://threedubmedia.com
 * Open Source MIT License - http://threedubmedia.com/code/license
 */
// Created: 2008-06-04
// Updated: 2012-05-21
// Updated: 2016-08-16   Luiz Gonzaga dos Santos Filho
// REQUIRES: jquery 1.8 +, , event.drag 2.3.0
// TESTED WITH: jQuery 1.8.3, 1.11.2, 2.2.4, and 3.1.0

; (function ($) { // secure $ jQuery alias

    // Events: drop, dropstart, dropend

    // add the jquery instance method
    $.fn.drop = function (str, arg, opts) {
        // figure out the event type
        var type = typeof str == "string" ? str : "",
        // figure out the event handler...
        fn = $.isFunction(str) ? str : $.isFunction(arg) ? arg : null;
        // fix the event type
        if (type.indexOf("drop") !== 0)
            type = "drop" + type;
        // were options passed
        opts = (str == fn ? arg : opts) || {};
        // trigger or bind event handler
        return fn ? this.on(type, opts, fn) : this.trigger(type);
    };

    // DROP MANAGEMENT UTILITY
    // returns filtered drop target elements, caches their positions
    $.drop = function (opts) {
        opts = opts || {};
        // safely set new options...
        drop.multi = opts.multi === true ? Infinity :
            opts.multi === false ? 1 : !isNaN(opts.multi) ? opts.multi : drop.multi;
        drop.delay = opts.delay || drop.delay;
        drop.tolerance = $.isFunction(opts.tolerance) ? opts.tolerance :
            opts.tolerance === null ? null : drop.tolerance;
        drop.mode = opts.mode || drop.mode || 'intersect';
    };

    // local refs (increase compression)
    var $event = $.event,
    $special = $event.special,
    // configure the drop special event
    drop = $.event.special.drop = {

        // these are the default settings
        multi: 1, // allow multiple drop winners per dragged element
        delay: 20, // async timeout delay
        mode: 'overlap', // drop tolerance mode

        // internal cache
        targets: [],

        // the key name for stored drop data
        datakey: "dropdata",

        // prevent bubbling for better performance
        noBubble: true,

        // count bound related events
        add: function (obj) {
            // read the interaction data
            var data = $.data(this, drop.datakey);
            // count another realted event
            data.related += 1;
        },

        // forget unbound related events
        remove: function () {
            $.data(this, drop.datakey).related -= 1;
        },

        // configure the interactions
        setup: function () {
            // check for related events
            if ($.data(this, drop.datakey))
                return;
            // initialize the drop element data
            var data = {
                related: 0,
                active: [],
                anyactive: 0,
                winner: 0,
                location: {}
            };
            // store the drop data on the element
            $.data(this, drop.datakey, data);
            // store the drop target in internal cache
            drop.targets.push(this);
        },

        // destroy the configure interaction
        teardown: function () {
            var data = $.data(this, drop.datakey) || {};
            // check for related events
            if (data.related)
                return;
            // remove the stored data
            $.removeData(this, drop.datakey);
            // reference the targeted element
            var element = this;
            // remove from the internal cache
            drop.targets = $.grep(drop.targets, function (target) {
                return (target !== element);
            });
        },

        // shared event handler
        handler: function (event, dd) {
            // local vars
            var results, $targets;
            // make sure the right data is available
            if (!dd)
                return;
            // handle various events
            switch (event.type) {
                // draginit, from $.event.special.drag
                case 'mousedown': // DROPINIT >>
                case 'touchstart': // DROPINIT >>
                    // collect and assign the drop targets
                    $targets = $(drop.targets);
                    if (typeof dd.drop == "string")
                        $targets = $targets.filter(dd.drop);
                    // reset drop data winner properties
                    $targets.each(function () {
                        var data = $.data(this, drop.datakey);
                        data.active = [];
                        data.anyactive = 0;
                        data.winner = 0;
                    });
                    // set available target elements
                    dd.droppable = $targets;
                    // activate drop targets for the initial element being dragged
                    $special.drag.hijack(event, "dropinit", dd);
                    break;
                    // drag, from $.event.special.drag
                case 'mousemove': // TOLERATE >>
                case 'touchmove': // TOLERATE >>
                    drop.event = event; // store the mousemove event
                    if (!drop.timer)
                        // monitor drop targets
                        drop.tolerate(dd);
                    break;
                    // dragend, from $.event.special.drag
                case 'mouseup': // DROP >> DROPEND >>
                case 'touchend': // DROP >> DROPEND >>
                    drop.timer = clearTimeout(drop.timer); // delete timer
                    if (dd.propagates) {
                        $special.drag.hijack(event, "drop", dd);
                        $special.drag.hijack(event, "dropend", dd);
                    }
                    break;

            }
        },

        // returns the location positions of an element
        locate: function (elem, index) {
            var data = $.data(elem, drop.datakey),
            $elem = $(elem),
            posi = $elem.offset() || {},
            height = $elem.outerHeight(),
            width = $elem.outerWidth(),
            location = {
                elem: elem,
                width: width,
                height: height,
                top: posi.top,
                left: posi.left,
                right: posi.left + width,
                bottom: posi.top + height
            };
            // drag elements might not have dropdata
            if (data) {
                data.location = location;
                data.index = index;
                data.elem = elem;
            }
            return location;
        },

        // test the location positions of an element against another OR an X,Y coord
        contains: function (target, test) { // target { location } contains test [x,y] or { location }
            return ((test[0] || test.left) >= target.left && (test[0] || test.right) <= target.right
                && (test[1] || test.top) >= target.top && (test[1] || test.bottom) <= target.bottom);
        },

        // stored tolerance modes
        modes: { // fn scope: "$.event.special.drop" object
            // target with mouse wins, else target with most overlap wins
            'intersect': function (event, proxy, target) {
                return this.contains(target, [event.pageX, event.pageY]) ? // check cursor
                    1e9 : this.modes.overlap.apply(this, arguments); // check overlap
            },
            // target with most overlap wins
            'overlap': function (event, proxy, target) {
                // calculate the area of overlap...
                return Math.max(0, Math.min(target.bottom, proxy.bottom) - Math.max(target.top, proxy.top))
                    * Math.max(0, Math.min(target.right, proxy.right) - Math.max(target.left, proxy.left));
            },
            // proxy is completely contained within target bounds
            'fit': function (event, proxy, target) {
                return this.contains(target, proxy) ? 1 : 0;
            },
            // center of the proxy is contained within target bounds
            'middle': function (event, proxy, target) {
                return this.contains(target, [proxy.left + proxy.width * .5, proxy.top + proxy.height * .5]) ? 1 : 0;
            }
        },

        // sort drop target cache by by winner (dsc), then index (asc)
        sort: function (a, b) {
            return (b.winner - a.winner) || (a.index - b.index);
        },

        // async, recursive tolerance execution
        tolerate: function (dd) {
            // declare local refs
            var i, drp, drg, data, arr, len, elem,
            // interaction iteration variables
            x = 0, ia, end = dd.interactions.length,
            // determine the mouse coords
            xy = [drop.event.pageX, drop.event.pageY],
            // custom or stored tolerance fn
            tolerance = drop.tolerance || drop.modes[drop.mode];
            // go through each passed interaction...
            do if (ia = dd.interactions[x]) {
                // check valid interaction
                if (!ia)
                    return;
                // initialize or clear the drop data
                ia.drop = [];
                // holds the drop elements
                arr = [];
                len = ia.droppable.length;
                // determine the proxy location, if needed
                if (tolerance)
                    drg = drop.locate(ia.proxy);
                // reset the loop
                i = 0;
                // loop each stored drop target
                do if (elem = ia.droppable[i]) {
                    data = $.data(elem, drop.datakey);
                    drp = data.location;
                    if (!drp) continue;
                    // find a winner: tolerance function is defined, call it
                    data.winner = tolerance ? tolerance.call(drop, drop.event, drg, drp)
                        // mouse position is always the fallback
                        : drop.contains(drp, xy) ? 1 : 0;
                    arr.push(data);
                } while (++i < len); // loop
                // sort the drop targets
                arr.sort(drop.sort);
                // reset the loop
                i = 0;
                // loop through all of the targets again
                do if (data = arr[i]) {
                    // winners...
                    if (data.winner && ia.drop.length < drop.multi) {
                        // new winner... dropstart
                        if (!data.active[x] && !data.anyactive) {
                            // check to make sure that this is not prevented
                            if ($special.drag.hijack(drop.event, "dropstart", dd, x, data.elem)[0] !== false) {
                                data.active[x] = 1;
                                data.anyactive += 1;
                            }
                                // if false, it is not a winner
                            else
                                data.winner = 0;
                        }
                        // if it is still a winner
                        if (data.winner)
                            ia.drop.push(data.elem);
                    }
                        // losers...
                    else if (data.active[x] && data.anyactive == 1) {
                        // former winner... dropend
                        $special.drag.hijack(drop.event, "dropend", dd, x, data.elem);
                        data.active[x] = 0;
                        data.anyactive -= 1;
                    }
                } while (++i < len); // loop
            } while (++x < end) // loop
            // check if the mouse is still moving or is idle
            if (drop.last && xy[0] == drop.last.pageX && xy[1] == drop.last.pageY)
                delete drop.timer; // idle, don't recurse
            else  // recurse
                drop.timer = setTimeout(function () {
                    drop.tolerate(dd);
                }, drop.delay);
            // remember event, to compare idleness
            drop.last = drop.event;
        }

    };

    // share the same special event configuration with related events...
    $special.dropinit = $special.dropstart = $special.dropend = drop;

})(jQuery); // confine scope

//#endregion End jquery.event.drop

//#region slick.core

/***
 * Contains core SlickGrid classes.
 * @module Core
 * @namespace Slick
 */

(function ($) {
    // register namespace
    $.extend(true, window, {
        "Slick": {
            "Event": Event,
            "EventData": EventData,
            "EventHandler": EventHandler,
            "Range": Range,
            "NonDataRow": NonDataItem,
            "Group": Group,
            "GroupTotals": GroupTotals,
            "EditorLock": EditorLock,

            /***
             * A global singleton editor lock.
             * @class GlobalEditorLock
             * @static
             * @constructor
             */
            "GlobalEditorLock": new EditorLock(),

            "keyCode": {
                BACKSPACE: 8,
                DELETE: 46,
                DOWN: 40,
                END: 35,
                ENTER: 13,
                ESCAPE: 27,
                HOME: 36,
                INSERT: 45,
                LEFT: 37,
                PAGE_DOWN: 34,
                PAGE_UP: 33,
                RIGHT: 39,
                TAB: 9,
                UP: 38
            }
        }
    });

    /***
     * An event object for passing data to event handlers and letting them control propagation.
     * <p>This is pretty much identical to how W3C and jQuery implement events.</p>
     * @class EventData
     * @constructor
     */
    function EventData() {
        var isPropagationStopped = false;
        var isImmediatePropagationStopped = false;

        /***
         * Stops event from propagating up the DOM tree.
         * @method stopPropagation
         */
        this.stopPropagation = function () {
            isPropagationStopped = true;
        };

        /***
         * Returns whether stopPropagation was called on this event object.
         * @method isPropagationStopped
         * @return {Boolean}
         */
        this.isPropagationStopped = function () {
            return isPropagationStopped;
        };

        /***
         * Prevents the rest of the handlers from being executed.
         * @method stopImmediatePropagation
         */
        this.stopImmediatePropagation = function () {
            isImmediatePropagationStopped = true;
        };

        /***
         * Returns whether stopImmediatePropagation was called on this event object.\
         * @method isImmediatePropagationStopped
         * @return {Boolean}
         */
        this.isImmediatePropagationStopped = function () {
            return isImmediatePropagationStopped;
        }
    }

    /***
     * A simple publisher-subscriber implementation.
     * @class Event
     * @constructor
     */
    function Event() {
        var handlers = [];

        /***
         * Adds an event handler to be called when the event is fired.
         * <p>Event handler will receive two arguments - an <code>EventData</code> and the <code>data</code>
         * object the event was fired with.<p>
         * @method subscribe
         * @param fn {Function} Event handler.
         */
        this.subscribe = function (fn) {
            handlers.push(fn);
        };

        /***
         * Removes an event handler added with <code>subscribe(fn)</code>.
         * @method unsubscribe
         * @param fn {Function} Event handler to be removed.
         */
        this.unsubscribe = function (fn) {
            for (var i = handlers.length - 1; i >= 0; i--) {
                if (handlers[i] === fn) {
                    handlers.splice(i, 1);
                }
            }
        };

        /***
         * Fires an event notifying all subscribers.
         * @method notify
         * @param args {Object} Additional data object to be passed to all handlers.
         * @param e {EventData}
         *      Optional.
         *      An <code>EventData</code> object to be passed to all handlers.
         *      For DOM events, an existing W3C/jQuery event object can be passed in.
         * @param scope {Object}
         *      Optional.
         *      The scope ("this") within which the handler will be executed.
         *      If not specified, the scope will be set to the <code>Event</code> instance.
         */
        this.notify = function (args, e, scope) {
            e = e || new EventData();
            scope = scope || this;

            var returnValue;
            for (var i = 0; i < handlers.length && !(e.isPropagationStopped() || e.isImmediatePropagationStopped()) ; i++) {
                returnValue = handlers[i].call(scope, e, args);
            }

            return returnValue;
        };
    }

    function EventHandler() {
        var handlers = [];

        this.subscribe = function (event, handler) {
            handlers.push({
                event: event,
                handler: handler
            });
            event.subscribe(handler);

            return this;  // allow chaining
        };

        this.unsubscribe = function (event, handler) {
            var i = handlers.length;
            while (i--) {
                if (handlers[i].event === event &&
                    handlers[i].handler === handler) {
                    handlers.splice(i, 1);
                    event.unsubscribe(handler);
                    return;
                }
            }

            return this;  // allow chaining
        };

        this.unsubscribeAll = function () {
            var i = handlers.length;
            while (i--) {
                handlers[i].event.unsubscribe(handlers[i].handler);
            }
            handlers = [];

            return this;  // allow chaining
        }
    }

    /***
     * A structure containing a range of cells.
     * @class Range
     * @constructor
     * @param fromRow {Integer} Starting row.
     * @param fromCell {Integer} Starting cell.
     * @param toRow {Integer} Optional. Ending row. Defaults to <code>fromRow</code>.
     * @param toCell {Integer} Optional. Ending cell. Defaults to <code>fromCell</code>.
     */
    function Range(fromRow, fromCell, toRow, toCell) {
        if (toRow === undefined && toCell === undefined) {
            toRow = fromRow;
            toCell = fromCell;
        }

        /***
         * @property fromRow
         * @type {Integer}
         */
        this.fromRow = Math.min(fromRow, toRow);

        /***
         * @property fromCell
         * @type {Integer}
         */
        this.fromCell = Math.min(fromCell, toCell);

        /***
         * @property toRow
         * @type {Integer}
         */
        this.toRow = Math.max(fromRow, toRow);

        /***
         * @property toCell
         * @type {Integer}
         */
        this.toCell = Math.max(fromCell, toCell);

        /***
         * Returns whether a range represents a single row.
         * @method isSingleRow
         * @return {Boolean}
         */
        this.isSingleRow = function () {
            return this.fromRow == this.toRow;
        };

        /***
         * Returns whether a range represents a single cell.
         * @method isSingleCell
         * @return {Boolean}
         */
        this.isSingleCell = function () {
            return this.fromRow == this.toRow && this.fromCell == this.toCell;
        };

        /***
         * Returns whether a range contains a given cell.
         * @method contains
         * @param row {Integer}
         * @param cell {Integer}
         * @return {Boolean}
         */
        this.contains = function (row, cell) {
            return row >= this.fromRow && row <= this.toRow &&
                cell >= this.fromCell && cell <= this.toCell;
        };

        /***
         * Returns a readable representation of a range.
         * @method toString
         * @return {String}
         */
        this.toString = function () {
            if (this.isSingleCell()) {
                return "(" + this.fromRow + ":" + this.fromCell + ")";
            }
            else {
                return "(" + this.fromRow + ":" + this.fromCell + " - " + this.toRow + ":" + this.toCell + ")";
            }
        }
    }


    /***
     * A base class that all special / non-data rows (like Group and GroupTotals) derive from.
     * @class NonDataItem
     * @constructor
     */
    function NonDataItem() {
        this.__nonDataRow = true;
    }


    /***
     * Information about a group of rows.
     * @class Group
     * @extends Slick.NonDataItem
     * @constructor
     */
    function Group() {
        this.__group = true;

        /**
         * Grouping level, starting with 0.
         * @property level
         * @type {Number}
         */
        this.level = 0;

        /***
         * Number of rows in the group.
         * @property count
         * @type {Integer}
         */
        this.count = 0;

        /***
         * Grouping value.
         * @property value
         * @type {Object}
         */
        this.value = null;

        /***
         * Formatted display value of the group.
         * @property title
         * @type {String}
         */
        this.title = null;

        /***
         * Whether a group is collapsed.
         * @property collapsed
         * @type {Boolean}
         */
        this.collapsed = false;

        /***
         * GroupTotals, if any.
         * @property totals
         * @type {GroupTotals}
         */
        this.totals = null;

        /**
         * Rows that are part of the group.
         * @property rows
         * @type {Array}
         */
        this.rows = [];

        /**
         * Sub-groups that are part of the group.
         * @property groups
         * @type {Array}
         */
        this.groups = null;

        /**
         * A unique key used to identify the group.  This key can be used in calls to DataView
         * collapseGroup() or expandGroup().
         * @property groupingKey
         * @type {Object}
         */
        this.groupingKey = null;
    }

    Group.prototype = new NonDataItem();

    /***
     * Compares two Group instances.
     * @method equals
     * @return {Boolean}
     * @param group {Group} Group instance to compare to.
     */
    Group.prototype.equals = function (group) {
        return this.value === group.value &&
            this.count === group.count &&
            this.collapsed === group.collapsed &&
            this.title === group.title;
    };

    /***
     * Information about group totals.
     * An instance of GroupTotals will be created for each totals row and passed to the aggregators
     * so that they can store arbitrary data in it.  That data can later be accessed by group totals
     * formatters during the display.
     * @class GroupTotals
     * @extends Slick.NonDataItem
     * @constructor
     */
    function GroupTotals() {
        this.__groupTotals = true;

        /***
         * Parent Group.
         * @param group
         * @type {Group}
         */
        this.group = null;

        /***
         * Whether the totals have been fully initialized / calculated.
         * Will be set to false for lazy-calculated group totals.
         * @param initialized
         * @type {Boolean}
         */
        this.initialized = false;
    }

    GroupTotals.prototype = new NonDataItem();

    /***
     * A locking helper to track the active edit controller and ensure that only a single controller
     * can be active at a time.  This prevents a whole class of state and validation synchronization
     * issues.  An edit controller (such as SlickGrid) can query if an active edit is in progress
     * and attempt a commit or cancel before proceeding.
     * @class EditorLock
     * @constructor
     */
    function EditorLock() {
        var activeEditController = null;

        /***
         * Returns true if a specified edit controller is active (has the edit lock).
         * If the parameter is not specified, returns true if any edit controller is active.
         * @method isActive
         * @param editController {EditController}
         * @return {Boolean}
         */
        this.isActive = function (editController) {
            return (editController ? activeEditController === editController : activeEditController !== null);
        };

        /***
         * Sets the specified edit controller as the active edit controller (acquire edit lock).
         * If another edit controller is already active, and exception will be thrown.
         * @method activate
         * @param editController {EditController} edit controller acquiring the lock
         */
        this.activate = function (editController) {
            if (editController === activeEditController) { // already activated?
                return;
            }
            if (activeEditController !== null) {
                throw "SlickGrid.EditorLock.activate: an editController is still active, can't activate another editController";
            }
            if (!editController.commitCurrentEdit) {
                throw "SlickGrid.EditorLock.activate: editController must implement .commitCurrentEdit()";
            }
            if (!editController.cancelCurrentEdit) {
                throw "SlickGrid.EditorLock.activate: editController must implement .cancelCurrentEdit()";
            }
            activeEditController = editController;
        };

        /***
         * Unsets the specified edit controller as the active edit controller (release edit lock).
         * If the specified edit controller is not the active one, an exception will be thrown.
         * @method deactivate
         * @param editController {EditController} edit controller releasing the lock
         */
        this.deactivate = function (editController) {
            if (activeEditController !== editController) {
                throw "SlickGrid.EditorLock.deactivate: specified editController is not the currently active one";
            }
            activeEditController = null;
        };

        /***
         * Attempts to commit the current edit by calling "commitCurrentEdit" method on the active edit
         * controller and returns whether the commit attempt was successful (commit may fail due to validation
         * errors, etc.).  Edit controller's "commitCurrentEdit" must return true if the commit has succeeded
         * and false otherwise.  If no edit controller is active, returns true.
         * @method commitCurrentEdit
         * @return {Boolean}
         */
        this.commitCurrentEdit = function () {
            return (activeEditController ? activeEditController.commitCurrentEdit() : true);
        };

        /***
         * Attempts to cancel the current edit by calling "cancelCurrentEdit" method on the active edit
         * controller and returns whether the edit was successfully cancelled.  If no edit controller is
         * active, returns true.
         * @method cancelCurrentEdit
         * @return {Boolean}
         */
        this.cancelCurrentEdit = function cancelCurrentEdit() {
            return (activeEditController ? activeEditController.cancelCurrentEdit() : true);
        };
    }
})(jQuery);

//#endregion End slick.core

//#region slick.grid

/**
 * @license
 * (c) 2009-2016 Michael Leibman
 * michael{dot}leibman{at}gmail{dot}com
 * http://github.com/mleibman/slickgrid
 *
 * Distributed under MIT license.
 * All rights reserved.
 *
 * SlickGrid v2.3
 *
 * NOTES:
 *     Cell/row DOM manipulations are done directly bypassing jQuery's DOM manipulation methods.
 *     This increases the speed dramatically, but can only be done safely because there are no event handlers
 *     or data associated with any cell/row DOM nodes.  Cell editors must make sure they implement .destroy()
 *     and do proper cleanup.
 */

// make sure required JavaScript modules are loaded
if (typeof jQuery === "undefined") {
    throw "SlickGrid requires jquery module to be loaded";
}
if (!jQuery.fn.drag) {
    throw "SlickGrid requires jquery.event.drag module to be loaded";
}
if (typeof Slick === "undefined") {
    throw "slick.core.js not loaded";
}


(function ($) {
    // Slick.Grid
    $.extend(true, window, {
        Slick: {
            Grid: SlickGrid
        }
    });

    // shared across all grids on the page
    var scrollbarDimensions;
    var maxSupportedCssHeight;  // browser's breaking point

    //////////////////////////////////////////////////////////////////////////////////////////////
    // SlickGrid class implementation (available as Slick.Grid)

    /**
     * Creates a new instance of the grid.
     * @class SlickGrid
     * @constructor
     * @param {Node}              container   Container node to create the grid in.
     * @param {Array,Object}      data        An array of objects for databinding.
     * @param {Array}             columns     An array of column definitions.
     * @param {Object}            options     Grid options.
     **/
    function SlickGrid(container, data, columns, options) {
        // settings
        var defaults = {
            explicitInitialization: false,
            rowHeight: 25,
            defaultColumnWidth: 80,
            enableAddRow: false,
            leaveSpaceForNewRows: false,
            editable: false,
            autoEdit: true,
            enableCellNavigation: true,
            enableColumnReorder: true,
            asyncEditorLoading: false,
            asyncEditorLoadDelay: 100,
            forceFitColumns: false,
            enableAsyncPostRender: false,
            asyncPostRenderDelay: 50,
            enableAsyncPostRenderCleanup: false,
            asyncPostRenderCleanupDelay: 40,
            autoHeight: false,
            editorLock: Slick.GlobalEditorLock,
            showHeaderRow: false,
            headerRowHeight: 25,
            createFooterRow: false,
            showFooterRow: false,
            footerRowHeight: 25,
            showTopPanel: false,
            topPanelHeight: 25,
            formatterFactory: null,
            editorFactory: null,
            cellFlashingCssClass: "flashing",
            selectedCellCssClass: "selected",
            multiSelect: true,
            enableTextSelectionOnCells: false,
            dataItemColumnValueExtractor: null,
            fullWidthRows: false,
            multiColumnSort: false,
            defaultFormatter: defaultFormatter,
            forceSyncScrolling: false,
            addNewRowCssClass: "new-row"
        };

        var columnDefaults = {
            name: "",
            resizable: true,
            sortable: false,
            minWidth: 30,
            rerenderOnResize: false,
            headerCssClass: null,
            defaultSortAsc: true,
            focusable: true,
            selectable: true
        };

        // scroller
        var th;   // virtual height
        var h;    // real scrollable height
        var ph;   // page height
        var n;    // number of pages
        var cj;   // "jumpiness" coefficient

        var page = 0;       // current page
        var offset = 0;     // current page offset
        var vScrollDir = 1;

        // private
        var initialized = false;
        var $container;
        var uid = "slickgrid_" + Math.round(1000000 * Math.random());
        var self = this;
        var $focusSink, $focusSink2;
        var $headerScroller;
        var $headers;
        var $headerRow, $headerRowScroller, $headerRowSpacer;
        var $footerRow, $footerRowScroller, $footerRowSpacer;
        var $topPanelScroller;
        var $topPanel;
        var $viewport;
        var $canvas;
        var $style;
        var $boundAncestors;
        var stylesheet, columnCssRulesL, columnCssRulesR;
        var viewportH, viewportW;
        var canvasWidth;
        var viewportHasHScroll, viewportHasVScroll;
        var headerColumnWidthDiff = 0, headerColumnHeightDiff = 0, // border+padding
            cellWidthDiff = 0, cellHeightDiff = 0, jQueryNewWidthBehaviour = false;
        var absoluteColumnMinWidth;

        var tabbingDirection = 1;
        var activePosX;
        var activeRow, activeCell;
        var activeCellNode = null;
        var currentEditor = null;
        var serializedEditorValue;
        var editController;

        var rowsCache = {};
        var renderedRows = 0;
        var numVisibleRows;
        var prevScrollTop = 0;
        var scrollTop = 0;
        var lastRenderedScrollTop = 0;
        var lastRenderedScrollLeft = 0;
        var prevScrollLeft = 0;
        var scrollLeft = 0;

        var selectionModel;
        var selectedRows = [];

        var plugins = [];
        var cellCssClasses = {};

        var columnsById = {};
        var sortColumns = [];
        var columnPosLeft = [];
        var columnPosRight = [];

        var pagingActive = false;
        var pagingIsLastPage = false;

        // async call handles
        var h_editorLoader = null;
        var h_render = null;
        var h_postrender = null;
        var h_postrenderCleanup = null;
        var postProcessedRows = {};
        var postProcessToRow = null;
        var postProcessFromRow = null;
        var postProcessedCleanupQueue = [];
        var postProcessgroupId = 0;

        // perf counters
        var counter_rows_rendered = 0;
        var counter_rows_removed = 0;

        // These two variables work around a bug with inertial scrolling in Webkit/Blink on Mac.
        // See http://crbug.com/312427.
        var rowNodeFromLastMouseWheelEvent;  // this node must not be deleted while inertial scrolling
        var zombieRowNodeFromLastMouseWheelEvent;  // node that was hidden instead of getting deleted
        var zombieRowCacheFromLastMouseWheelEvent;  // row cache for above node
        var zombieRowPostProcessedFromLastMouseWheelEvent;  // post processing references for above node

        // store css attributes if display:none is active in container or parent
        var cssShow = { position: 'absolute', visibility: 'hidden', display: 'block' };
        var $hiddenParents;
        var oldProps = [];

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Initialization

        function init() {
            $container = $(container);
            if ($container.length < 1) {
                throw new Error("SlickGrid requires a valid container, " + container + " does not exist in the DOM.");
            }

            cacheCssForHiddenInit();

            // calculate these only once and share between grid instances
            maxSupportedCssHeight = maxSupportedCssHeight || getMaxSupportedCssHeight();
            scrollbarDimensions = scrollbarDimensions || measureScrollbar();

            options = $.extend({}, defaults, options);
            validateAndEnforceOptions();
            columnDefaults.width = options.defaultColumnWidth;

            columnsById = {};
            for (var i = 0; i < columns.length; i++) {
                var m = columns[i] = $.extend({}, columnDefaults, columns[i]);
                columnsById[m.id] = i;
                if (m.minWidth && m.width < m.minWidth) {
                    m.width = m.minWidth;
                }
                if (m.maxWidth && m.width > m.maxWidth) {
                    m.width = m.maxWidth;
                }
            }

            // validate loaded JavaScript modules against requested options
            if (options.enableColumnReorder && !$.fn.sortable) {
                throw new Error("SlickGrid's 'enableColumnReorder = true' option requires jquery-ui.sortable module to be loaded");
            }

            editController = {
                "commitCurrentEdit": commitCurrentEdit,
                "cancelCurrentEdit": cancelCurrentEdit
            };

            $container
                .empty()
                .css("overflow", "hidden")
                .css("outline", 0)
                .addClass(uid)
                .addClass("ui-widget");

            // set up a positioning container if needed
            if (!/relative|absolute|fixed/.test($container.css("position"))) {
                $container.css("position", "relative");
            }

            $focusSink = $("<div tabIndex='0' hideFocus style='position:fixed;width:0;height:0;top:0;left:0;outline:0;'></div>").appendTo($container);

            $headerScroller = $("<div class='slick-header ui-state-default' style='overflow:hidden;position:relative;' />").appendTo($container);
            $headers = $("<div class='slick-header-columns' style='left:-1000px' />").appendTo($headerScroller);
            $headers.width(getHeadersWidth());

            $headerRowScroller = $("<div class='slick-headerrow ui-state-default' style='overflow:hidden;position:relative;' />").appendTo($container);
            $headerRow = $("<div class='slick-headerrow-columns' />").appendTo($headerRowScroller);
            $headerRowSpacer = $("<div style='display:block;height:1px;position:absolute;top:0;left:0;'></div>")
                .css("width", getCanvasWidth() + scrollbarDimensions.width + "px")
                .appendTo($headerRowScroller);

            $topPanelScroller = $("<div class='slick-top-panel-scroller ui-state-default' style='overflow:hidden;position:relative;' />").appendTo($container);
            $topPanel = $("<div class='slick-top-panel' style='width:10000px' />").appendTo($topPanelScroller);

            if (!options.showTopPanel) {
                $topPanelScroller.hide();
            }

            if (!options.showHeaderRow) {
                $headerRowScroller.hide();
            }

            $viewport = $("<div class='slick-viewport' style='width:100%;overflow:auto;outline:0;position:relative;;'>").appendTo($container);
            $viewport.css("overflow-y", options.autoHeight ? "hidden" : "auto");

            $canvas = $("<div class='grid-canvas' />").appendTo($viewport);

            if (options.createFooterRow) {
                $footerRowScroller = $("<div class='slick-footerrow ui-state-default' style='overflow:hidden;position:relative;' />").appendTo($container);
                $footerRow = $("<div class='slick-footerrow-columns' />").appendTo($footerRowScroller);
                $footerRowSpacer = $("<div style='display:block;height:1px;position:absolute;top:0;left:0;'></div>")
                    .css("width", getCanvasWidth() + scrollbarDimensions.width + "px")
                    .appendTo($footerRowScroller);

                if (!options.showFooterRow) {
                    $footerRowScroller.hide();
                }
            }

            $focusSink2 = $focusSink.clone().appendTo($container);

            if (!options.explicitInitialization) {
                finishInitialization();
            }
        }

        function finishInitialization() {
            if (!initialized) {
                initialized = true;

                viewportW = parseFloat($.css($container[0], "width", true));

                // header columns and cells may have different padding/border skewing width calculations (box-sizing, hello?)
                // calculate the diff so we can set consistent sizes
                measureCellPaddingAndBorder();

                // for usability reasons, all text selection in SlickGrid is disabled
                // with the exception of input and textarea elements (selection must
                // be enabled there so that editors work as expected); note that
                // selection in grid cells (grid body) is already unavailable in
                // all browsers except IE
                disableSelection($headers); // disable all text selection in header (including input and textarea)

                if (!options.enableTextSelectionOnCells) {
                    // disable text selection in grid cells except in input and textarea elements
                    // (this is IE-specific, because selectstart event will only fire in IE)
                    $viewport.on("selectstart.ui", function (event) {
                        return $(event.target).is("input,textarea");
                    });
                }

                updateColumnCaches();
                createColumnHeaders();
                setupColumnSort();
                createCssRules();
                resizeCanvas();
                bindAncestorScrollEvents();

                $container
                    .on("resize.slickgrid", resizeCanvas);
                $viewport
                    //.on("click", handleClick)
                    .on("scroll", handleScroll);
                $headerScroller
                    .on("contextmenu", handleHeaderContextMenu)
                    .on("click", handleHeaderClick)
                    .on("mouseenter", ".slick-header-column", handleHeaderMouseEnter)
                    .on("mouseleave", ".slick-header-column", handleHeaderMouseLeave);
                $headerRowScroller
                    .on("scroll", handleHeaderRowScroll);

                if (options.createFooterRow) {
                    $footerRowScroller
                        .on("scroll", handleFooterRowScroll);
                }

                $focusSink.add($focusSink2)
                    .on("keydown", handleKeyDown);
                $canvas
                    .on("keydown", handleKeyDown)
                    .on("click", handleClick)
                    .on("dblclick", handleDblClick)
                    .on("contextmenu", handleContextMenu)
                    .on("draginit", handleDragInit)
                    .on("dragstart", { distance: 3 }, handleDragStart)
                    .on("drag", handleDrag)
                    .on("dragend", handleDragEnd)
                    .on("mouseenter", ".slick-cell", handleMouseEnter)
                    .on("mouseleave", ".slick-cell", handleMouseLeave);

                // Work around http://crbug.com/312427.
                if (navigator.userAgent.toLowerCase().match(/webkit/) &&
                    navigator.userAgent.toLowerCase().match(/macintosh/)) {
                    $canvas.on("mousewheel", handleMouseWheel);
                }
                restoreCssFromHiddenInit();
            }
        }

        function cacheCssForHiddenInit() {
            // handle display:none on container or container parents
            $hiddenParents = $container.parents().addBack().not(':visible');
            $hiddenParents.each(function () {
                var old = {};
                for (var name in cssShow) {
                    old[name] = this.style[name];
                    this.style[name] = cssShow[name];
                }
                oldProps.push(old);
            });
        }

        function restoreCssFromHiddenInit() {
            // finish handle display:none on container or container parents
            // - put values back the way they were
            $hiddenParents.each(function (i) {
                var old = oldProps[i];
                for (var name in cssShow) {
                    this.style[name] = old[name];
                }
            });
        }

        function registerPlugin(plugin) {
            plugins.unshift(plugin);
            plugin.init(self);
        }

        function unregisterPlugin(plugin) {
            for (var i = plugins.length; i >= 0; i--) {
                if (plugins[i] === plugin) {
                    if (plugins[i].destroy) {
                        plugins[i].destroy();
                    }
                    plugins.splice(i, 1);
                    break;
                }
            }
        }

        function setSelectionModel(model) {
            if (selectionModel) {
                selectionModel.onSelectedRangesChanged.unsubscribe(handleSelectedRangesChanged);
                if (selectionModel.destroy) {
                    selectionModel.destroy();
                }
            }

            selectionModel = model;
            if (selectionModel) {
                selectionModel.init(self);
                selectionModel.onSelectedRangesChanged.subscribe(handleSelectedRangesChanged);
            }
        }

        function getSelectionModel() {
            return selectionModel;
        }

        function getCanvasNode() {
            return $canvas[0];
        }

        function measureScrollbar() {
            var $c = $("<div style='position:absolute; top:-10000px; left:-10000px; width:100px; height:100px; overflow:scroll;'></div>").appendTo("body");
            var dim = {
                width: $c.width() - $c[0].clientWidth,
                height: $c.height() - $c[0].clientHeight
            };
            $c.remove();
            return dim;
        }

        function getHeadersWidth() {
            var headersWidth = 0;
            for (var i = 0, ii = columns.length; i < ii; i++) {
                var width = columns[i].width;
                headersWidth += width;
            }
            headersWidth += scrollbarDimensions.width;
            return Math.max(headersWidth, viewportW) + 1000;
        }

        function getCanvasWidth() {
            var availableWidth = viewportHasVScroll ? viewportW - scrollbarDimensions.width : viewportW;
            var rowWidth = 0;
            var i = columns.length;
            while (i--) {
                rowWidth += columns[i].width;
            }
            return options.fullWidthRows ? Math.max(rowWidth, availableWidth) : rowWidth;
        }

        function updateCanvasWidth(forceColumnWidthsUpdate) {
            var oldCanvasWidth = canvasWidth;
            canvasWidth = getCanvasWidth();

            if (canvasWidth != oldCanvasWidth) {
                $canvas.width(canvasWidth);
                $headerRow.width(canvasWidth);
                if (options.createFooterRow) { $footerRow.width(canvasWidth); }
                $headers.width(getHeadersWidth());
                viewportHasHScroll = (canvasWidth > viewportW - scrollbarDimensions.width);
            }

            var w = canvasWidth + (viewportHasVScroll ? scrollbarDimensions.width : 0);
            $headerRowSpacer.width(w);
            if (options.createFooterRow) { $footerRowSpacer.width(w); }

            if (canvasWidth != oldCanvasWidth || forceColumnWidthsUpdate) {
                applyColumnWidths();
            }
        }

        function disableSelection($target) {
            if ($target && $target.jquery) {
                $target
                    .attr("unselectable", "on")
                    .css("MozUserSelect", "none")
                    .on("selectstart.ui", function () {
                        return false;
                    }); // from jquery:ui.core.js 1.7.2
            }
        }

        function getMaxSupportedCssHeight() {
            var supportedHeight = 1000000;
            // FF reports the height back but still renders blank after ~6M px
            var testUpTo = navigator.userAgent.toLowerCase().match(/firefox/) ? 6000000 : 1000000000;
            var div = $("<div style='display:none' />").appendTo(document.body);

            while (true) {
                var test = supportedHeight * 2;
                div.css("height", test);
                if (test > testUpTo || div.height() !== test) {
                    break;
                } else {
                    supportedHeight = test;
                }
            }

            div.remove();
            return supportedHeight;
        }

        // TODO:  this is static.  need to handle page mutation.
        function bindAncestorScrollEvents() {
            var elem = $canvas[0];
            while ((elem = elem.parentNode) != document.body && elem != null) {
                // bind to scroll containers only
                if (elem == $viewport[0] || elem.scrollWidth != elem.clientWidth || elem.scrollHeight != elem.clientHeight) {
                    var $elem = $(elem);
                    if (!$boundAncestors) {
                        $boundAncestors = $elem;
                    } else {
                        $boundAncestors = $boundAncestors.add($elem);
                    }
                    $elem.on("scroll." + uid, handleActiveCellPositionChange);
                }
            }
        }

        function unbindAncestorScrollEvents() {
            if (!$boundAncestors) {
                return;
            }
            $boundAncestors.off("scroll." + uid);
            $boundAncestors = null;
        }

        function updateColumnHeader(columnId, title, toolTip) {
            if (!initialized) { return; }
            var idx = getColumnIndex(columnId);
            if (idx == null) {
                return;
            }

            var columnDef = columns[idx];
            var $header = $headers.children().eq(idx);
            if ($header) {
                if (title !== undefined) {
                    columns[idx].name = title;
                }
                if (toolTip !== undefined) {
                    columns[idx].toolTip = toolTip;
                }

                trigger(self.onBeforeHeaderCellDestroy, {
                    "node": $header[0],
                    "column": columnDef,
                    "grid": self
                });

                $header
                    .attr("title", toolTip || "")
                    .children().eq(0).html(title);

                trigger(self.onHeaderCellRendered, {
                    "node": $header[0],
                    "column": columnDef,
                    "grid": self
                });
            }
        }

        function getHeaderRow() {
            return $headerRow[0];
        }

        function getFooterRow() {
            return $footerRow[0];
        }

        function getHeaderRowColumn(columnId) {
            var idx = getColumnIndex(columnId);
            var $header = $headerRow.children().eq(idx);
            return $header && $header[0];
        }

        function getFooterRowColumn(columnId) {
            var idx = getColumnIndex(columnId);
            var $footer = $footerRow.children().eq(idx);
            return $footer && $footer[0];
        }

        function createColumnHeaders() {
            function onMouseEnter() {
                $(this).addClass("ui-state-hover");
            }

            function onMouseLeave() {
                $(this).removeClass("ui-state-hover");
            }

            $headers.find(".slick-header-column")
              .each(function () {
                  var columnDef = $(this).data("column");
                  if (columnDef) {
                      trigger(self.onBeforeHeaderCellDestroy, {
                          "node": this,
                          "column": columnDef,
                          "grid": self
                      });
                  }
              });
            $headers.empty();
            $headers.width(getHeadersWidth());

            $headerRow.find(".slick-headerrow-column")
              .each(function () {
                  var columnDef = $(this).data("column");
                  if (columnDef) {
                      trigger(self.onBeforeHeaderRowCellDestroy, {
                          "node": this,
                          "column": columnDef,
                          "grid": self
                      });
                  }
              });
            $headerRow.empty();

            if (options.createFooterRow) {
                $footerRow.find(".slick-footerrow-column")
                  .each(function () {
                      var columnDef = $(this).data("column");
                      if (columnDef) {
                          trigger(self.onBeforeFooterRowCellDestroy, {
                              "node": this,
                              "column": columnDef
                          });
                      }
                  });
                $footerRow.empty();
            }

            for (var i = 0; i < columns.length; i++) {
                var m = columns[i];

                var header = $("<div class='ui-state-default slick-header-column' />")
                    .html("<span class='slick-column-name'>" + m.name + "</span>")
                    .width(m.width - headerColumnWidthDiff)
                    .attr("id", "" + uid + m.id)
                    .attr("title", m.toolTip || "")
                    .data("column", m)
                    .addClass(m.headerCssClass || "")
                    .appendTo($headers);

                if (options.enableColumnReorder || m.sortable) {
                    header
                      .on('mouseenter', onMouseEnter)
                      .on('mouseleave', onMouseLeave);
                }

                if (m.sortable) {
                    header.addClass("slick-header-sortable");
                    header.append("<span class='slick-sort-indicator' />");
                }

                trigger(self.onHeaderCellRendered, {
                    "node": header[0],
                    "column": m,
                    "grid": self
                });

                if (options.showHeaderRow) {
                    var headerRowCell = $("<div class='ui-state-default slick-headerrow-column l" + i + " r" + i + "'></div>")
                        .data("column", m)
                        .appendTo($headerRow);

                    trigger(self.onHeaderRowCellRendered, {
                        "node": headerRowCell[0],
                        "column": m,
                        "grid": self
                    });
                }
                if (options.createFooterRow && options.showFooterRow) {
                    var footerRowCell = $("<div class='ui-state-default slick-footerrow-column l" + i + " r" + i + "'></div>")
                        .data("column", m)
                        .appendTo($footerRow);

                    trigger(self.onFooterRowCellRendered, {
                        "node": footerRowCell[0],
                        "column": m
                    });
                }
            }

            setSortColumns(sortColumns);
            setupColumnResize();
            if (options.enableColumnReorder) {
                setupColumnReorder();
            }
        }

        function setupColumnSort() {
            $headers.click(function (e) {
                // temporary workaround for a bug in jQuery 1.7.1 (http://bugs.jquery.com/ticket/11328)
                e.metaKey = e.metaKey || e.ctrlKey;

                if ($(e.target).hasClass("slick-resizable-handle")) {
                    return;
                }

                var $col = $(e.target).closest(".slick-header-column");
                if (!$col.length) {
                    return;
                }

                var column = $col.data("column");
                if (column.sortable) {
                    if (!getEditorLock().commitCurrentEdit()) {
                        return;
                    }

                    var sortOpts = null;
                    var i = 0;
                    for (; i < sortColumns.length; i++) {
                        if (sortColumns[i].columnId == column.id) {
                            sortOpts = sortColumns[i];
                            sortOpts.sortAsc = !sortOpts.sortAsc;
                            break;
                        }
                    }

                    if (e.metaKey && options.multiColumnSort) {
                        if (sortOpts) {
                            sortColumns.splice(i, 1);
                        }
                    }
                    else {
                        if ((!e.shiftKey && !e.metaKey) || !options.multiColumnSort) {
                            sortColumns = [];
                        }

                        if (!sortOpts) {
                            sortOpts = { columnId: column.id, sortAsc: column.defaultSortAsc };
                            sortColumns.push(sortOpts);
                        } else if (sortColumns.length == 0) {
                            sortColumns.push(sortOpts);
                        }
                    }

                    setSortColumns(sortColumns);

                    if (!options.multiColumnSort) {
                        trigger(self.onSort, {
                            multiColumnSort: false,
                            sortCol: column,
                            sortAsc: sortOpts.sortAsc,
                            grid: self
                        }, e);
                    } else {
                        trigger(self.onSort, {
                            multiColumnSort: true,
                            sortCols: $.map(sortColumns, function (col) {
                                return { sortCol: columns[getColumnIndex(col.columnId)], sortAsc: col.sortAsc };
                            }),
                            grid: self
                        }, e);
                    }
                }
            });
        }

        function setupColumnReorder() {
            $headers.filter(":ui-sortable").sortable("destroy");
            $headers.sortable({
                containment: "parent",
                distance: 3,
                axis: "x",
                cursor: "default",
                tolerance: "intersection",
                helper: "clone",
                placeholder: "slick-sortable-placeholder ui-state-default slick-header-column",
                start: function (e, ui) {
                    ui.placeholder.width(ui.helper.outerWidth() - headerColumnWidthDiff);
                    $(ui.helper).addClass("slick-header-column-active");
                },
                beforeStop: function (e, ui) {
                    $(ui.helper).removeClass("slick-header-column-active");
                },
                stop: function (e) {
                    if (!getEditorLock().commitCurrentEdit()) {
                        $(this).sortable("cancel");
                        return;
                    }

                    var reorderedIds = $headers.sortable("toArray");
                    var reorderedColumns = [];
                    for (var i = 0; i < reorderedIds.length; i++) {
                        reorderedColumns.push(columns[getColumnIndex(reorderedIds[i].replace(uid, ""))]);
                    }
                    setColumns(reorderedColumns);

                    trigger(self.onColumnsReordered, { grid: self });
                    e.stopPropagation();
                    setupColumnResize();
                }
            });
        }

        function setupColumnResize() {
            var $col, j, c, pageX, columnElements, minPageX, maxPageX, firstResizable, lastResizable;
            columnElements = $headers.children();
            columnElements.find(".slick-resizable-handle").remove();
            columnElements.each(function (i, e) {
                if (columns[i].resizable) {
                    if (firstResizable === undefined) {
                        firstResizable = i;
                    }
                    lastResizable = i;
                }
            });
            if (firstResizable === undefined) {
                return;
            }
            columnElements.each(function (i, e) {
                if (i < firstResizable || (options.forceFitColumns && i >= lastResizable)) {
                    return;
                }
                $col = $(e);
                $("<div class='slick-resizable-handle' />")
                    .appendTo(e)
                    .on("dragstart", function (e, dd) {
                        if (!getEditorLock().commitCurrentEdit()) {
                            return false;
                        }
                        pageX = e.pageX;
                        $(this).parent().addClass("slick-header-column-active");
                        var shrinkLeewayOnRight = null, stretchLeewayOnRight = null;
                        // lock each column's width option to current width
                        columnElements.each(function (i, e) {
                            columns[i].previousWidth = $(e).outerWidth();
                        });
                        if (options.forceFitColumns) {
                            shrinkLeewayOnRight = 0;
                            stretchLeewayOnRight = 0;
                            // colums on right affect maxPageX/minPageX
                            for (j = i + 1; j < columnElements.length; j++) {
                                c = columns[j];
                                if (c.resizable) {
                                    if (stretchLeewayOnRight !== null) {
                                        if (c.maxWidth) {
                                            stretchLeewayOnRight += c.maxWidth - c.previousWidth;
                                        } else {
                                            stretchLeewayOnRight = null;
                                        }
                                    }
                                    shrinkLeewayOnRight += c.previousWidth - Math.max(c.minWidth || 0, absoluteColumnMinWidth);
                                }
                            }
                        }
                        var shrinkLeewayOnLeft = 0, stretchLeewayOnLeft = 0;
                        for (j = 0; j <= i; j++) {
                            // columns on left only affect minPageX
                            c = columns[j];
                            if (c.resizable) {
                                if (stretchLeewayOnLeft !== null) {
                                    if (c.maxWidth) {
                                        stretchLeewayOnLeft += c.maxWidth - c.previousWidth;
                                    } else {
                                        stretchLeewayOnLeft = null;
                                    }
                                }
                                shrinkLeewayOnLeft += c.previousWidth - Math.max(c.minWidth || 0, absoluteColumnMinWidth);
                            }
                        }
                        if (shrinkLeewayOnRight === null) {
                            shrinkLeewayOnRight = 100000;
                        }
                        if (shrinkLeewayOnLeft === null) {
                            shrinkLeewayOnLeft = 100000;
                        }
                        if (stretchLeewayOnRight === null) {
                            stretchLeewayOnRight = 100000;
                        }
                        if (stretchLeewayOnLeft === null) {
                            stretchLeewayOnLeft = 100000;
                        }
                        maxPageX = pageX + Math.min(shrinkLeewayOnRight, stretchLeewayOnLeft);
                        minPageX = pageX - Math.min(shrinkLeewayOnLeft, stretchLeewayOnRight);
                    })
                    .on("drag", function (e, dd) {
                        var actualMinWidth, d = Math.min(maxPageX, Math.max(minPageX, e.pageX)) - pageX, x;
                        if (d < 0) { // shrink column
                            x = d;
                            for (j = i; j >= 0; j--) {
                                c = columns[j];
                                if (c.resizable) {
                                    actualMinWidth = Math.max(c.minWidth || 0, absoluteColumnMinWidth);
                                    if (x && c.previousWidth + x < actualMinWidth) {
                                        x += c.previousWidth - actualMinWidth;
                                        c.width = actualMinWidth;
                                    } else {
                                        c.width = c.previousWidth + x;
                                        x = 0;
                                    }
                                }
                            }

                            if (options.forceFitColumns) {
                                x = -d;
                                for (j = i + 1; j < columnElements.length; j++) {
                                    c = columns[j];
                                    if (c.resizable) {
                                        if (x && c.maxWidth && (c.maxWidth - c.previousWidth < x)) {
                                            x -= c.maxWidth - c.previousWidth;
                                            c.width = c.maxWidth;
                                        } else {
                                            c.width = c.previousWidth + x;
                                            x = 0;
                                        }
                                    }
                                }
                            }
                        } else { // stretch column
                            x = d;
                            for (j = i; j >= 0; j--) {
                                c = columns[j];
                                if (c.resizable) {
                                    if (x && c.maxWidth && (c.maxWidth - c.previousWidth < x)) {
                                        x -= c.maxWidth - c.previousWidth;
                                        c.width = c.maxWidth;
                                    } else {
                                        c.width = c.previousWidth + x;
                                        x = 0;
                                    }
                                }
                            }

                            if (options.forceFitColumns) {
                                x = -d;
                                for (j = i + 1; j < columnElements.length; j++) {
                                    c = columns[j];
                                    if (c.resizable) {
                                        actualMinWidth = Math.max(c.minWidth || 0, absoluteColumnMinWidth);
                                        if (x && c.previousWidth + x < actualMinWidth) {
                                            x += c.previousWidth - actualMinWidth;
                                            c.width = actualMinWidth;
                                        } else {
                                            c.width = c.previousWidth + x;
                                            x = 0;
                                        }
                                    }
                                }
                            }
                        }
                        applyColumnHeaderWidths();
                        if (options.syncColumnCellResize) {
                            applyColumnWidths();
                        }
                    })
                    .on("dragend", function (e, dd) {
                        var newWidth;
                        $(this).parent().removeClass("slick-header-column-active");
                        for (j = 0; j < columnElements.length; j++) {
                            c = columns[j];
                            newWidth = $(columnElements[j]).outerWidth();

                            if (c.previousWidth !== newWidth && c.rerenderOnResize) {
                                invalidateAllRows();
                            }
                        }
                        updateCanvasWidth(true);
                        render();
                        trigger(self.onColumnsResized, { grid: self });
                    });
            });
        }

        function getVBoxDelta($el) {
            var p = ["borderTopWidth", "borderBottomWidth", "paddingTop", "paddingBottom"];
            var delta = 0;
            $.each(p, function (n, val) {
                delta += parseFloat($el.css(val)) || 0;
            });
            return delta;
        }

        function measureCellPaddingAndBorder() {
            var el;
            var h = ["borderLeftWidth", "borderRightWidth", "paddingLeft", "paddingRight"];
            var v = ["borderTopWidth", "borderBottomWidth", "paddingTop", "paddingBottom"];

            // jquery prior to version 1.8 handles .width setter/getter as a direct css write/read
            // jquery 1.8 changed .width to read the true inner element width if box-sizing is set to border-box, and introduced a setter for .outerWidth
            // so for equivalent functionality, prior to 1.8 use .width, and after use .outerWidth
            var verArray = $.fn.jquery.split('.');
            jQueryNewWidthBehaviour = (verArray[0] == 1 && verArray[1] >= 8) || verArray[0] >= 2;

            el = $("<div class='ui-state-default slick-header-column' style='visibility:hidden'>-</div>").appendTo($headers);
            headerColumnWidthDiff = headerColumnHeightDiff = 0;
            if (el.css("box-sizing") != "border-box" && el.css("-moz-box-sizing") != "border-box" && el.css("-webkit-box-sizing") != "border-box") {
                $.each(h, function (n, val) {
                    headerColumnWidthDiff += parseFloat(el.css(val)) || 0;
                });
                $.each(v, function (n, val) {
                    headerColumnHeightDiff += parseFloat(el.css(val)) || 0;
                });
            }
            el.remove();

            var r = $("<div class='slick-row' />").appendTo($canvas);
            el = $("<div class='slick-cell' id='' style='visibility:hidden'>-</div>").appendTo(r);
            cellWidthDiff = cellHeightDiff = 0;
            if (el.css("box-sizing") != "border-box" && el.css("-moz-box-sizing") != "border-box" && el.css("-webkit-box-sizing") != "border-box") {
                $.each(h, function (n, val) {
                    cellWidthDiff += parseFloat(el.css(val)) || 0;
                });
                $.each(v, function (n, val) {
                    cellHeightDiff += parseFloat(el.css(val)) || 0;
                });
            }
            r.remove();

            absoluteColumnMinWidth = Math.max(headerColumnWidthDiff, cellWidthDiff);
        }

        function createCssRules() {
            $style = $("<style type='text/css' rel='stylesheet' />").appendTo($("head"));
            var rowHeight = (options.rowHeight - cellHeightDiff);
            var rules = [
              "." + uid + " .slick-header-column { left: 1000px; }",
              "." + uid + " .slick-top-panel { height:" + options.topPanelHeight + "px; }",
              "." + uid + " .slick-headerrow-columns { height:" + options.headerRowHeight + "px; }",
              "." + uid + " .slick-footerrow-columns { height:" + options.footerRowHeight + "px; }",
              "." + uid + " .slick-cell { height:" + rowHeight + "px; }",
              "." + uid + " .slick-row { height:" + options.rowHeight + "px; }"
            ];

            for (var i = 0; i < columns.length; i++) {
                rules.push("." + uid + " .l" + i + " { }");
                rules.push("." + uid + " .r" + i + " { }");
            }

            if ($style[0].styleSheet) { // IE
                $style[0].styleSheet.cssText = rules.join(" ");
            } else {
                $style[0].appendChild(document.createTextNode(rules.join(" ")));
            }
        }

        function getColumnCssRules(idx) {
            if (!stylesheet) {
                var sheets = document.styleSheets;
                for (var i = 0; i < sheets.length; i++) {
                    if ((sheets[i].ownerNode || sheets[i].owningElement) == $style[0]) {
                        stylesheet = sheets[i];
                        break;
                    }
                }

                if (!stylesheet) {
                    throw new Error("Cannot find stylesheet.");
                }

                // find and cache column CSS rules
                columnCssRulesL = [];
                columnCssRulesR = [];
                var cssRules = (stylesheet.cssRules || stylesheet.rules);
                var matches, columnIdx;
                for (var i = 0; i < cssRules.length; i++) {
                    var selector = cssRules[i].selectorText;
                    if (matches = /\.l\d+/.exec(selector)) {
                        columnIdx = parseInt(matches[0].substr(2, matches[0].length - 2), 10);
                        columnCssRulesL[columnIdx] = cssRules[i];
                    } else if (matches = /\.r\d+/.exec(selector)) {
                        columnIdx = parseInt(matches[0].substr(2, matches[0].length - 2), 10);
                        columnCssRulesR[columnIdx] = cssRules[i];
                    }
                }
            }

            return {
                "left": columnCssRulesL[idx],
                "right": columnCssRulesR[idx]
            };
        }

        function removeCssRules() {
            $style.remove();
            stylesheet = null;
        }

        function destroy() {
            getEditorLock().cancelCurrentEdit();

            trigger(self.onBeforeDestroy, { grid: self });

            var i = plugins.length;
            while (i--) {
                unregisterPlugin(plugins[i]);
            }

            if (options.enableColumnReorder) {
                $headers.filter(":ui-sortable").sortable("destroy");
            }

            unbindAncestorScrollEvents();
            $container.off(".slickgrid");
            removeCssRules();

            $canvas.off("draginit dragstart dragend drag");
            $container.empty().removeClass(uid);
        }


        //////////////////////////////////////////////////////////////////////////////////////////////
        // General

        function trigger(evt, args, e) {
            e = e || new Slick.EventData();
            args = args || {};
            args.grid = self;
            return evt.notify(args, e, self);
        }

        function getEditorLock() {
            return options.editorLock;
        }

        function getEditController() {
            return editController;
        }

        function getColumnIndex(id) {
            return columnsById[id];
        }

        function autosizeColumns() {
            var i, c,
                widths = [],
                shrinkLeeway = 0,
                total = 0,
                prevTotal,
                availWidth = viewportHasVScroll ? viewportW - scrollbarDimensions.width : viewportW;

            for (i = 0; i < columns.length; i++) {
                c = columns[i];
                widths.push(c.width);
                total += c.width;
                if (c.resizable) {
                    shrinkLeeway += c.width - Math.max(c.minWidth, absoluteColumnMinWidth);
                }
            }

            // shrink
            prevTotal = total;
            while (total > availWidth && shrinkLeeway) {
                var shrinkProportion = (total - availWidth) / shrinkLeeway;
                for (i = 0; i < columns.length && total > availWidth; i++) {
                    c = columns[i];
                    var width = widths[i];
                    if (!c.resizable || width <= c.minWidth || width <= absoluteColumnMinWidth) {
                        continue;
                    }
                    var absMinWidth = Math.max(c.minWidth, absoluteColumnMinWidth);
                    var shrinkSize = Math.floor(shrinkProportion * (width - absMinWidth)) || 1;
                    shrinkSize = Math.min(shrinkSize, width - absMinWidth);
                    total -= shrinkSize;
                    shrinkLeeway -= shrinkSize;
                    widths[i] -= shrinkSize;
                }
                if (prevTotal <= total) {  // avoid infinite loop
                    break;
                }
                prevTotal = total;
            }

            // grow
            prevTotal = total;
            while (total < availWidth) {
                var growProportion = availWidth / total;
                for (i = 0; i < columns.length && total < availWidth; i++) {
                    c = columns[i];
                    var currentWidth = widths[i];
                    var growSize;

                    if (!c.resizable || c.maxWidth <= currentWidth) {
                        growSize = 0;
                    } else {
                        growSize = Math.min(Math.floor(growProportion * currentWidth) - currentWidth, (c.maxWidth - currentWidth) || 1000000) || 1;
                    }
                    total += growSize;
                    widths[i] += (total <= availWidth ? growSize : 0);
                }
                if (prevTotal >= total) {  // avoid infinite loop
                    break;
                }
                prevTotal = total;
            }

            var reRender = false;
            for (i = 0; i < columns.length; i++) {
                if (columns[i].rerenderOnResize && columns[i].width != widths[i]) {
                    reRender = true;
                }
                columns[i].width = widths[i];
            }

            applyColumnHeaderWidths();
            updateCanvasWidth(true);
            if (reRender) {
                invalidateAllRows();
                render();
            }
        }

        function applyColumnHeaderWidths() {
            if (!initialized) { return; }
            var h;
            for (var i = 0, headers = $headers.children(), ii = headers.length; i < ii; i++) {
                h = $(headers[i]);
                if (jQueryNewWidthBehaviour) {
                    if (h.outerWidth() !== columns[i].width) {
                        h.outerWidth(columns[i].width);
                    }
                } else {
                    if (h.width() !== columns[i].width - headerColumnWidthDiff) {
                        h.width(columns[i].width - headerColumnWidthDiff);
                    }
                }
            }

            updateColumnCaches();
        }

        function applyColumnWidths() {
            var x = 0, w, rule;
            for (var i = 0; i < columns.length; i++) {
                w = columns[i].width;

                rule = getColumnCssRules(i);
                rule.left.style.left = x + "px";
                rule.right.style.right = (canvasWidth - x - w) + "px";

                x += columns[i].width;
            }
        }

        function setSortColumn(columnId, ascending) {
            setSortColumns([{ columnId: columnId, sortAsc: ascending }]);
        }

        function setSortColumns(cols) {
            sortColumns = cols;

            var headerColumnEls = $headers.children();
            headerColumnEls
                .removeClass("slick-header-column-sorted")
                .find(".slick-sort-indicator")
                    .removeClass("slick-sort-indicator-asc slick-sort-indicator-desc");

            $.each(sortColumns, function (i, col) {
                if (col.sortAsc == null) {
                    col.sortAsc = true;
                }
                var columnIndex = getColumnIndex(col.columnId);
                if (columnIndex != null) {
                    headerColumnEls.eq(columnIndex)
                        .addClass("slick-header-column-sorted")
                        .find(".slick-sort-indicator")
                            .addClass(col.sortAsc ? "slick-sort-indicator-asc" : "slick-sort-indicator-desc");
                }
            });
        }

        function getSortColumns() {
            return sortColumns;
        }

        function handleSelectedRangesChanged(e, ranges) {
            selectedRows = [];
            var hash = {};
            for (var i = 0; i < ranges.length; i++) {
                for (var j = ranges[i].fromRow; j <= ranges[i].toRow; j++) {
                    if (!hash[j]) {  // prevent duplicates
                        selectedRows.push(j);
                        hash[j] = {};
                    }
                    for (var k = ranges[i].fromCell; k <= ranges[i].toCell; k++) {
                        if (canCellBeSelected(j, k)) {
                            hash[j][columns[k].id] = options.selectedCellCssClass;
                        }
                    }
                }
            }

            setCellCssStyles(options.selectedCellCssClass, hash);

            trigger(self.onSelectedRowsChanged, { rows: getSelectedRows(), grid: self }, e);
        }

        function getColumns() {
            return columns;
        }

        function updateColumnCaches() {
            // Pre-calculate cell boundaries.
            columnPosLeft = [];
            columnPosRight = [];
            var x = 0;
            for (var i = 0, ii = columns.length; i < ii; i++) {
                columnPosLeft[i] = x;
                columnPosRight[i] = x + columns[i].width;
                x += columns[i].width;
            }
        }

        function setColumns(columnDefinitions) {
            columns = columnDefinitions;

            columnsById = {};
            for (var i = 0; i < columns.length; i++) {
                var m = columns[i] = $.extend({}, columnDefaults, columns[i]);
                columnsById[m.id] = i;
                if (m.minWidth && m.width < m.minWidth) {
                    m.width = m.minWidth;
                }
                if (m.maxWidth && m.width > m.maxWidth) {
                    m.width = m.maxWidth;
                }
            }

            updateColumnCaches();

            if (initialized) {
                invalidateAllRows();
                createColumnHeaders();
                removeCssRules();
                createCssRules();
                resizeCanvas();
                applyColumnWidths();
                handleScroll();
            }
        }

        function getOptions() {
            return options;
        }

        function setOptions(args) {
            if (!getEditorLock().commitCurrentEdit()) {
                return;
            }

            makeActiveCellNormal();

            if (options.enableAddRow !== args.enableAddRow) {
                invalidateRow(getDataLength());
            }

            options = $.extend(options, args);
            validateAndEnforceOptions();

            $viewport.css("overflow-y", options.autoHeight ? "hidden" : "auto");
            render();
        }

        function validateAndEnforceOptions() {
            if (options.autoHeight) {
                options.leaveSpaceForNewRows = false;
            }
        }

        function setData(newData, scrollToTop) {
            data = newData;
            invalidateAllRows();
            updateRowCount();
            if (scrollToTop) {
                scrollTo(0);
            }
        }

        function getData() {
            return data;
        }

        function getDataLength() {
            if (data.getLength) {
                return data.getLength();
            } else {
                return data.length;
            }
        }

        function getDataLengthIncludingAddNew() {
            return getDataLength() + (!options.enableAddRow ? 0
              : (!pagingActive || pagingIsLastPage ? 1 : 0)
            );
        }

        function getDataItem(i) {
            if (data.getItem) {
                return data.getItem(i);
            } else {
                return data[i];
            }
        }

        function getTopPanel() {
            return $topPanel[0];
        }

        function setTopPanelVisibility(visible) {
            if (options.showTopPanel != visible) {
                options.showTopPanel = visible;
                if (visible) {
                    $topPanelScroller.slideDown("fast", resizeCanvas);
                } else {
                    $topPanelScroller.slideUp("fast", resizeCanvas);
                }
            }
        }

        function setHeaderRowVisibility(visible) {
            if (options.showHeaderRow != visible) {
                options.showHeaderRow = visible;
                if (visible) {
                    $headerRowScroller.slideDown("fast", resizeCanvas);
                } else {
                    $headerRowScroller.slideUp("fast", resizeCanvas);
                }
            }
        }

        function setFooterRowVisibility(visible) {
            if (options.showFooterRow != visible) {
                options.showFooterRow = visible;
                if (visible) {
                    $footerRowScroller.slideDown("fast", resizeCanvas);
                } else {
                    $footerRowScroller.slideUp("fast", resizeCanvas);
                }
            }
        }

        function getContainerNode() {
            return $container.get(0);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Rendering / Scrolling

        function getRowTop(row) {
            return options.rowHeight * row - offset;
        }

        function getRowFromPosition(y) {
            return Math.floor((y + offset) / options.rowHeight);
        }

        function scrollTo(y) {
            y = Math.max(y, 0);
            y = Math.min(y, th - viewportH + (viewportHasHScroll ? scrollbarDimensions.height : 0));

            var oldOffset = offset;

            page = Math.min(n - 1, Math.floor(y / ph));
            offset = Math.round(page * cj);
            var newScrollTop = y - offset;

            if (offset != oldOffset) {
                var range = getVisibleRange(newScrollTop);
                cleanupRows(range);
                updateRowPositions();
            }

            if (prevScrollTop != newScrollTop) {
                vScrollDir = (prevScrollTop + oldOffset < newScrollTop + offset) ? 1 : -1;
                $viewport[0].scrollTop = (lastRenderedScrollTop = scrollTop = prevScrollTop = newScrollTop);

                trigger(self.onViewportChanged, { grid: self });
            }
        }

        function defaultFormatter(row, cell, value, columnDef, dataContext) {
            if (value == null) {
                return "";
            } else {
                return (value + "").replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            }
        }

        function getFormatter(row, column) {
            var rowMetadata = data.getItemMetadata && data.getItemMetadata(row);

            // look up by id, then index
            var columnOverrides = rowMetadata &&
                rowMetadata.columns &&
                (rowMetadata.columns[column.id] || rowMetadata.columns[getColumnIndex(column.id)]);

            return (columnOverrides && columnOverrides.formatter) ||
                (rowMetadata && rowMetadata.formatter) ||
                column.formatter ||
                (options.formatterFactory && options.formatterFactory.getFormatter(column)) ||
                options.defaultFormatter;
        }

        function getEditor(row, cell) {
            var column = columns[cell];
            var rowMetadata = data.getItemMetadata && data.getItemMetadata(row);
            var columnMetadata = rowMetadata && rowMetadata.columns;

            if (columnMetadata && columnMetadata[column.id] && columnMetadata[column.id].editor !== undefined) {
                return columnMetadata[column.id].editor;
            }
            if (columnMetadata && columnMetadata[cell] && columnMetadata[cell].editor !== undefined) {
                return columnMetadata[cell].editor;
            }

            return column.editor || (options.editorFactory && options.editorFactory.getEditor(column));
        }

        function getDataItemValueForColumn(item, columnDef) {
            if (options.dataItemColumnValueExtractor) {
                return options.dataItemColumnValueExtractor(item, columnDef);
            }
            return item[columnDef.field];
        }

        function appendRowHtml(stringArray, row, range, dataLength) {
            var d = getDataItem(row);
            var dataLoading = row < dataLength && !d;
            var rowCss = "slick-row" +
                (dataLoading ? " loading" : "") +
                (row === activeRow ? " active" : "") +
                (row % 2 == 1 ? " odd" : " even");

            if (!d) {
                rowCss += " " + options.addNewRowCssClass;
            }

            var metadata = data.getItemMetadata && data.getItemMetadata(row);

            if (metadata && metadata.cssClasses) {
                rowCss += " " + metadata.cssClasses;
            }

            stringArray.push("<div class='ui-widget-content " + rowCss + "' style='top:" + getRowTop(row) + "px'>");

            var colspan, m;
            for (var i = 0, ii = columns.length; i < ii; i++) {
                m = columns[i];
                colspan = 1;
                if (metadata && metadata.columns) {
                    var columnData = metadata.columns[m.id] || metadata.columns[i];
                    colspan = (columnData && columnData.colspan) || 1;
                    if (colspan === "*") {
                        colspan = ii - i;
                    }
                }

                // Do not render cells outside of the viewport.
                if (columnPosRight[Math.min(ii - 1, i + colspan - 1)] > range.leftPx) {
                    if (columnPosLeft[i] > range.rightPx) {
                        // All columns to the right are outside the range.
                        break;
                    }

                    appendCellHtml(stringArray, row, i, colspan, d);
                }

                if (colspan > 1) {
                    i += (colspan - 1);
                }
            }

            stringArray.push("</div>");
        }

        function appendCellHtml(stringArray, row, cell, colspan, item) {
            // stringArray: stringBuilder containing the HTML parts
            // row, cell: row and column index
            // colspan: HTML colspan
            // item: grid data for row

            var m = columns[cell];
            var cellCss = "slick-cell l" + cell + " r" + Math.min(columns.length - 1, cell + colspan - 1) +
                (m.cssClass ? " " + m.cssClass : "");
            if (row === activeRow && cell === activeCell) {
                cellCss += (" active");
            }

            // TODO:  merge them together in the setter
            for (var key in cellCssClasses) {
                if (cellCssClasses[key][row] && cellCssClasses[key][row][m.id]) {
                    cellCss += (" " + cellCssClasses[key][row][m.id]);
                }
            }

            stringArray.push("<div class='" + cellCss + "'>");

            // if there is a corresponding row (if not, this is the Add New row or this data hasn't been loaded yet)
            if (item) {
                var value = getDataItemValueForColumn(item, m);
                stringArray.push(getFormatter(row, m)(row, cell, value, m, item));
            }

            stringArray.push("</div>");

            rowsCache[row].cellRenderQueue.push(cell);
            rowsCache[row].cellColSpans[cell] = colspan;
        }


        function cleanupRows(rangeToKeep) {
            for (var i in rowsCache) {
                if (((i = parseInt(i, 10)) !== activeRow) && (i < rangeToKeep.top || i > rangeToKeep.bottom)) {
                    removeRowFromCache(i);
                }
            }
            if (options.enableAsyncPostRenderCleanup) { startPostProcessingCleanup(); }
        }

        function invalidate() {
            updateRowCount();
            invalidateAllRows();
            render();
        }

        function invalidateAllRows() {
            if (currentEditor) {
                makeActiveCellNormal();
            }
            for (var row in rowsCache) {
                removeRowFromCache(row);
            }
            if (options.enableAsyncPostRenderCleanup) { startPostProcessingCleanup(); }
        }

        function queuePostProcessedRowForCleanup(cacheEntry, postProcessedRow, rowIdx) {
            postProcessgroupId++;

            // store and detach node for later async cleanup
            for (var columnIdx in postProcessedRow) {
                if (postProcessedRow.hasOwnProperty(columnIdx)) {
                    postProcessedCleanupQueue.push({
                        actionType: 'C',
                        groupId: postProcessgroupId,
                        node: cacheEntry.cellNodesByColumnIdx[columnIdx | 0],
                        columnIdx: columnIdx | 0,
                        rowIdx: rowIdx
                    });
                }
            }
            postProcessedCleanupQueue.push({
                actionType: 'R',
                groupId: postProcessgroupId,
                node: cacheEntry.rowNode
            });
            $(cacheEntry.rowNode).detach();
        }

        function queuePostProcessedCellForCleanup(cellnode, columnIdx, rowIdx) {
            postProcessedCleanupQueue.push({
                actionType: 'C',
                groupId: postProcessgroupId,
                node: cellnode,
                columnIdx: columnIdx,
                rowIdx: rowIdx
            });
            $(cellnode).detach();
        }

        function removeRowFromCache(row) {
            var cacheEntry = rowsCache[row];
            if (!cacheEntry) {
                return;
            }

            if (rowNodeFromLastMouseWheelEvent === cacheEntry.rowNode) {
                cacheEntry.rowNode.style.display = 'none';
                zombieRowNodeFromLastMouseWheelEvent = rowNodeFromLastMouseWheelEvent;
                zombieRowCacheFromLastMouseWheelEvent = cacheEntry;
                zombieRowPostProcessedFromLastMouseWheelEvent = postProcessedRows[row];
                // ignore post processing cleanup in this case - it will be dealt with later
            } else {
                if (options.enableAsyncPostRenderCleanup && postProcessedRows[row]) {
                    queuePostProcessedRowForCleanup(cacheEntry, postProcessedRows[row], row);
                } else {
                    $canvas[0].removeChild(cacheEntry.rowNode);
                }
            }

            delete rowsCache[row];
            delete postProcessedRows[row];
            renderedRows--;
            counter_rows_removed++;
        }

        function invalidateRows(rows) {
            var i, rl;
            if (!rows || !rows.length) {
                return;
            }
            vScrollDir = 0;
            rl = rows.length;
            for (i = 0; i < rl; i++) {
                if (currentEditor && activeRow === rows[i]) {
                    makeActiveCellNormal();
                }
                if (rowsCache[rows[i]]) {
                    removeRowFromCache(rows[i]);
                }
            }
            if (options.enableAsyncPostRenderCleanup) { startPostProcessingCleanup(); }

        }

        function invalidateRow(row) {
            invalidateRows([row]);
        }

        function updateCell(row, cell) {
            var cellNode = getCellNode(row, cell);
            if (!cellNode) {
                return;
            }

            var m = columns[cell], d = getDataItem(row);
            if (currentEditor && activeRow === row && activeCell === cell) {
                currentEditor.loadValue(d);
            } else {
                cellNode.innerHTML = d ? getFormatter(row, m)(row, cell, getDataItemValueForColumn(d, m), m, d) : "";
                invalidatePostProcessingResults(row);
            }
        }

        function updateRow(row) {
            var cacheEntry = rowsCache[row];
            if (!cacheEntry) {
                return;
            }

            ensureCellNodesInRowsCache(row);

            var d = getDataItem(row);

            for (var columnIdx in cacheEntry.cellNodesByColumnIdx) {
                if (!cacheEntry.cellNodesByColumnIdx.hasOwnProperty(columnIdx)) {
                    continue;
                }

                columnIdx = columnIdx | 0;
                var m = columns[columnIdx],
                    node = cacheEntry.cellNodesByColumnIdx[columnIdx];

                if (row === activeRow && columnIdx === activeCell && currentEditor) {
                    currentEditor.loadValue(d);
                } else if (d) {
                    node.innerHTML = getFormatter(row, m)(row, columnIdx, getDataItemValueForColumn(d, m), m, d);
                } else {
                    node.innerHTML = "";
                }
            }

            invalidatePostProcessingResults(row);
        }

        function getViewportHeight() {
            return parseFloat($.css($container[0], "height", true)) -
                parseFloat($.css($container[0], "paddingTop", true)) -
                parseFloat($.css($container[0], "paddingBottom", true)) -
                parseFloat($.css($headerScroller[0], "height")) - getVBoxDelta($headerScroller) -
                (options.showTopPanel ? options.topPanelHeight + getVBoxDelta($topPanelScroller) : 0) -
                (options.showHeaderRow ? options.headerRowHeight + getVBoxDelta($headerRowScroller) : 0) -
                (options.createFooterRow && options.showFooterRow ? options.footerRowHeight + getVBoxDelta($footerRowScroller) : 0);
        }

        function resizeCanvas() {
            if (!initialized) { return; }
            if (options.autoHeight) {
                viewportH = options.rowHeight * getDataLengthIncludingAddNew();
            } else {
                viewportH = getViewportHeight();
            }

            numVisibleRows = Math.ceil(viewportH / options.rowHeight);
            viewportW = parseFloat($.css($container[0], "width", true));
            if (!options.autoHeight) {
                $viewport.height(viewportH);
            }

            if (options.forceFitColumns) {
                autosizeColumns();
            }

            updateRowCount();
            handleScroll();
            // Since the width has changed, force the render() to reevaluate virtually rendered cells.
            lastRenderedScrollLeft = -1;
            render();
        }

        function updatePagingStatusFromView(pagingInfo) {
            pagingActive = (pagingInfo.pageSize !== 0);
            pagingIsLastPage = (pagingInfo.pageNum == pagingInfo.totalPages - 1);
        }

        function updateRowCount() {
            if (!initialized) { return; }

            var dataLength = getDataLength();
            var dataLengthIncludingAddNew = getDataLengthIncludingAddNew();
            var numberOfRows = dataLengthIncludingAddNew +
                (options.leaveSpaceForNewRows ? numVisibleRows - 1 : 0);

            var oldViewportHasVScroll = viewportHasVScroll;
            // with autoHeight, we do not need to accommodate the vertical scroll bar
            viewportHasVScroll = !options.autoHeight && (numberOfRows * options.rowHeight > viewportH);
            viewportHasHScroll = (canvasWidth > viewportW - scrollbarDimensions.width);

            makeActiveCellNormal();

            // remove the rows that are now outside of the data range
            // this helps avoid redundant calls to .removeRow() when the size of the data decreased by thousands of rows
            var r1 = dataLength - 1;
            for (var i in rowsCache) {
                if (i > r1) {
                    removeRowFromCache(i);
                }
            }
            if (options.enableAsyncPostRenderCleanup) { startPostProcessingCleanup(); }

            if (activeCellNode && activeRow > r1) {
                resetActiveCell();
            }

            var oldH = h;
            th = Math.max(options.rowHeight * numberOfRows, viewportH - scrollbarDimensions.height);
            if (th < maxSupportedCssHeight) {
                // just one page
                h = ph = th;
                n = 1;
                cj = 0;
            } else {
                // break into pages
                h = maxSupportedCssHeight;
                ph = h / 100;
                n = Math.floor(th / ph);
                cj = (th - h) / (n - 1);
            }

            if (h !== oldH) {
                $canvas.css("height", h);
                scrollTop = $viewport[0].scrollTop;
            }

            var oldScrollTopInRange = (scrollTop + offset <= th - viewportH);

            if (th == 0 || scrollTop == 0) {
                page = offset = 0;
            } else if (oldScrollTopInRange) {
                // maintain virtual position
                scrollTo(scrollTop + offset);
            } else {
                // scroll to bottom
                scrollTo(th - viewportH);
            }

            if (h != oldH && options.autoHeight) {
                resizeCanvas();
            }

            if (options.forceFitColumns && oldViewportHasVScroll != viewportHasVScroll) {
                autosizeColumns();
            }
            updateCanvasWidth(false);
        }

        function getVisibleRange(viewportTop, viewportLeft) {
            if (viewportTop == null) {
                viewportTop = scrollTop;
            }
            if (viewportLeft == null) {
                viewportLeft = scrollLeft;
            }

            return {
                top: getRowFromPosition(viewportTop),
                bottom: getRowFromPosition(viewportTop + viewportH) + 1,
                leftPx: viewportLeft,
                rightPx: viewportLeft + viewportW
            };
        }

        function getRenderedRange(viewportTop, viewportLeft) {
            var range = getVisibleRange(viewportTop, viewportLeft);
            var buffer = Math.round(viewportH / options.rowHeight);
            var minBuffer = 3;

            if (vScrollDir == -1) {
                range.top -= buffer;
                range.bottom += minBuffer;
            } else if (vScrollDir == 1) {
                range.top -= minBuffer;
                range.bottom += buffer;
            } else {
                range.top -= minBuffer;
                range.bottom += minBuffer;
            }

            range.top = Math.max(0, range.top);
            range.bottom = Math.min(getDataLengthIncludingAddNew() - 1, range.bottom);

            range.leftPx -= viewportW;
            range.rightPx += viewportW;

            range.leftPx = Math.max(0, range.leftPx);
            range.rightPx = Math.min(canvasWidth, range.rightPx);

            return range;
        }

        function ensureCellNodesInRowsCache(row) {
            var cacheEntry = rowsCache[row];
            if (cacheEntry) {
                if (cacheEntry.cellRenderQueue.length) {
                    var lastChild = cacheEntry.rowNode.lastChild;
                    while (cacheEntry.cellRenderQueue.length) {
                        var columnIdx = cacheEntry.cellRenderQueue.pop();
                        cacheEntry.cellNodesByColumnIdx[columnIdx] = lastChild;
                        lastChild = lastChild.previousSibling;
                    }
                }
            }
        }

        function cleanUpCells(range, row) {
            var totalCellsRemoved = 0;
            var cacheEntry = rowsCache[row];

            // Remove cells outside the range.
            var cellsToRemove = [];
            for (var i in cacheEntry.cellNodesByColumnIdx) {
                // I really hate it when people mess with Array.prototype.
                if (!cacheEntry.cellNodesByColumnIdx.hasOwnProperty(i)) {
                    continue;
                }

                // This is a string, so it needs to be cast back to a number.
                i = i | 0;

                var colspan = cacheEntry.cellColSpans[i];
                if (columnPosLeft[i] > range.rightPx ||
                  columnPosRight[Math.min(columns.length - 1, i + colspan - 1)] < range.leftPx) {
                    if (!(row == activeRow && i == activeCell)) {
                        cellsToRemove.push(i);
                    }
                }
            }

            var cellToRemove, node;
            postProcessgroupId++;
            while ((cellToRemove = cellsToRemove.pop()) != null) {
                node = cacheEntry.cellNodesByColumnIdx[cellToRemove];
                if (options.enableAsyncPostRenderCleanup && postProcessedRows[row] && postProcessedRows[row][cellToRemove]) {
                    queuePostProcessedCellForCleanup(node, cellToRemove, row);
                } else {
                    cacheEntry.rowNode.removeChild(node);
                }

                delete cacheEntry.cellColSpans[cellToRemove];
                delete cacheEntry.cellNodesByColumnIdx[cellToRemove];
                if (postProcessedRows[row]) {
                    delete postProcessedRows[row][cellToRemove];
                }
                totalCellsRemoved++;
            }
        }

        function cleanUpAndRenderCells(range) {
            var cacheEntry;
            var stringArray = [];
            var processedRows = [];
            var cellsAdded;
            var totalCellsAdded = 0;
            var colspan;

            for (var row = range.top, btm = range.bottom; row <= btm; row++) {
                cacheEntry = rowsCache[row];
                if (!cacheEntry) {
                    continue;
                }

                // cellRenderQueue populated in renderRows() needs to be cleared first
                ensureCellNodesInRowsCache(row);

                cleanUpCells(range, row);

                // Render missing cells.
                cellsAdded = 0;

                var metadata = data.getItemMetadata && data.getItemMetadata(row);
                metadata = metadata && metadata.columns;

                var d = getDataItem(row);

                // TODO:  shorten this loop (index? heuristics? binary search?)
                for (var i = 0, ii = columns.length; i < ii; i++) {
                    // Cells to the right are outside the range.
                    if (columnPosLeft[i] > range.rightPx) {
                        break;
                    }

                    // Already rendered.
                    if ((colspan = cacheEntry.cellColSpans[i]) != null) {
                        i += (colspan > 1 ? colspan - 1 : 0);
                        continue;
                    }

                    colspan = 1;
                    if (metadata) {
                        var columnData = metadata[columns[i].id] || metadata[i];
                        colspan = (columnData && columnData.colspan) || 1;
                        if (colspan === "*") {
                            colspan = ii - i;
                        }
                    }

                    if (columnPosRight[Math.min(ii - 1, i + colspan - 1)] > range.leftPx) {
                        appendCellHtml(stringArray, row, i, colspan, d);
                        cellsAdded++;
                    }

                    i += (colspan > 1 ? colspan - 1 : 0);
                }

                if (cellsAdded) {
                    totalCellsAdded += cellsAdded;
                    processedRows.push(row);
                }
            }

            if (!stringArray.length) {
                return;
            }

            var x = document.createElement("div");
            x.innerHTML = stringArray.join("");

            var processedRow;
            var node;
            while ((processedRow = processedRows.pop()) != null) {
                cacheEntry = rowsCache[processedRow];
                var columnIdx;
                while ((columnIdx = cacheEntry.cellRenderQueue.pop()) != null) {
                    node = x.lastChild;
                    cacheEntry.rowNode.appendChild(node);
                    cacheEntry.cellNodesByColumnIdx[columnIdx] = node;
                }
            }
        }

        function renderRows(range) {
            var parentNode = $canvas[0],
                stringArray = [],
                rows = [],
                needToReselectCell = false,
                dataLength = getDataLength();

            for (var i = range.top, ii = range.bottom; i <= ii; i++) {
                if (rowsCache[i]) {
                    continue;
                }
                renderedRows++;
                rows.push(i);

                // Create an entry right away so that appendRowHtml() can
                // start populatating it.
                rowsCache[i] = {
                    "rowNode": null,

                    // ColSpans of rendered cells (by column idx).
                    // Can also be used for checking whether a cell has been rendered.
                    "cellColSpans": [],

                    // Cell nodes (by column idx).  Lazy-populated by ensureCellNodesInRowsCache().
                    "cellNodesByColumnIdx": [],

                    // Column indices of cell nodes that have been rendered, but not yet indexed in
                    // cellNodesByColumnIdx.  These are in the same order as cell nodes added at the
                    // end of the row.
                    "cellRenderQueue": []
                };

                appendRowHtml(stringArray, i, range, dataLength);
                if (activeCellNode && activeRow === i) {
                    needToReselectCell = true;
                }
                counter_rows_rendered++;
            }

            if (!rows.length) { return; }

            var x = document.createElement("div");
            x.innerHTML = stringArray.join("");

            for (var i = 0, ii = rows.length; i < ii; i++) {
                rowsCache[rows[i]].rowNode = parentNode.appendChild(x.firstChild);
            }

            if (needToReselectCell) {
                activeCellNode = getCellNode(activeRow, activeCell);
            }
        }

        function startPostProcessing() {
            if (!options.enableAsyncPostRender) {
                return;
            }
            clearTimeout(h_postrender);
            h_postrender = setTimeout(asyncPostProcessRows, options.asyncPostRenderDelay);
        }

        function startPostProcessingCleanup() {
            if (!options.enableAsyncPostRenderCleanup) {
                return;
            }
            clearTimeout(h_postrenderCleanup);
            h_postrenderCleanup = setTimeout(asyncPostProcessCleanupRows, options.asyncPostRenderCleanupDelay);
        }

        function invalidatePostProcessingResults(row) {
            // change status of columns to be re-rendered
            for (var columnIdx in postProcessedRows[row]) {
                if (postProcessedRows[row].hasOwnProperty(columnIdx)) {
                    postProcessedRows[row][columnIdx] = 'C';
                }
            }
            postProcessFromRow = Math.min(postProcessFromRow, row);
            postProcessToRow = Math.max(postProcessToRow, row);
            startPostProcessing();
        }

        function updateRowPositions() {
            for (var row in rowsCache) {
                rowsCache[row].rowNode.style.top = getRowTop(row) + "px";
            }
        }

        function render() {
            if (!initialized) { return; }
            var visible = getVisibleRange();
            var rendered = getRenderedRange();

            // remove rows no longer in the viewport
            cleanupRows(rendered);

            // add new rows & missing cells in existing rows
            if (lastRenderedScrollLeft != scrollLeft) {
                cleanUpAndRenderCells(rendered);
            }

            // render missing rows
            renderRows(rendered);

            postProcessFromRow = visible.top;
            postProcessToRow = Math.min(getDataLengthIncludingAddNew() - 1, visible.bottom);
            startPostProcessing();

            lastRenderedScrollTop = scrollTop;
            lastRenderedScrollLeft = scrollLeft;
            h_render = null;
        }

        function handleHeaderRowScroll() {
            var scrollLeft = $headerRowScroller[0].scrollLeft;
            if (scrollLeft != $viewport[0].scrollLeft) {
                $viewport[0].scrollLeft = scrollLeft;
            }
        }

        function handleFooterRowScroll() {
            var scrollLeft = $footerRowScroller[0].scrollLeft;
            if (scrollLeft != $viewport[0].scrollLeft) {
                $viewport[0].scrollLeft = scrollLeft;
            }
        }

        function handleScroll() {
            scrollTop = $viewport[0].scrollTop;
            scrollLeft = $viewport[0].scrollLeft;
            var vScrollDist = Math.abs(scrollTop - prevScrollTop);
            var hScrollDist = Math.abs(scrollLeft - prevScrollLeft);

            if (hScrollDist) {
                prevScrollLeft = scrollLeft;
                $headerScroller[0].scrollLeft = scrollLeft;
                $topPanelScroller[0].scrollLeft = scrollLeft;
                $headerRowScroller[0].scrollLeft = scrollLeft;
                if (options.createFooterRow) {
                    $footerRowScroller[0].scrollLeft = scrollLeft;
                }
            }

            if (vScrollDist) {
                vScrollDir = prevScrollTop < scrollTop ? 1 : -1;
                prevScrollTop = scrollTop;

                // switch virtual pages if needed
                if (vScrollDist < viewportH) {
                    scrollTo(scrollTop + offset);
                } else {
                    var oldOffset = offset;
                    if (h == viewportH) {
                        page = 0;
                    } else {
                        page = Math.min(n - 1, Math.floor(scrollTop * ((th - viewportH) / (h - viewportH)) * (1 / ph)));
                    }
                    offset = Math.round(page * cj);
                    if (oldOffset != offset) {
                        invalidateAllRows();
                    }
                }
            }

            if (hScrollDist || vScrollDist) {
                if (h_render) {
                    clearTimeout(h_render);
                }

                if (Math.abs(lastRenderedScrollTop - scrollTop) > 20 ||
                    Math.abs(lastRenderedScrollLeft - scrollLeft) > 20) {
                    if (options.forceSyncScrolling || (
                        Math.abs(lastRenderedScrollTop - scrollTop) < viewportH &&
                        Math.abs(lastRenderedScrollLeft - scrollLeft) < viewportW)) {
                        render();
                    } else {
                        h_render = setTimeout(render, 50);
                    }

                    trigger(self.onViewportChanged, { grid: self });
                }
            }

            trigger(self.onScroll, { scrollLeft: scrollLeft, scrollTop: scrollTop, grid: self });
        }

        function asyncPostProcessRows() {
            var dataLength = getDataLength();
            while (postProcessFromRow <= postProcessToRow) {
                var row = (vScrollDir >= 0) ? postProcessFromRow++ : postProcessToRow--;
                var cacheEntry = rowsCache[row];
                if (!cacheEntry || row >= dataLength) {
                    continue;
                }

                if (!postProcessedRows[row]) {
                    postProcessedRows[row] = {};
                }

                ensureCellNodesInRowsCache(row);
                for (var columnIdx in cacheEntry.cellNodesByColumnIdx) {
                    if (!cacheEntry.cellNodesByColumnIdx.hasOwnProperty(columnIdx)) {
                        continue;
                    }

                    columnIdx = columnIdx | 0;

                    var m = columns[columnIdx];
                    var processedStatus = postProcessedRows[row][columnIdx]; // C=cleanup and re-render, R=rendered
                    if (m.asyncPostRender && processedStatus !== 'R') {
                        var node = cacheEntry.cellNodesByColumnIdx[columnIdx];
                        if (node) {
                            m.asyncPostRender(node, row, getDataItem(row), m, (processedStatus === 'C'));
                        }
                        postProcessedRows[row][columnIdx] = 'R';
                    }
                }

                h_postrender = setTimeout(asyncPostProcessRows, options.asyncPostRenderDelay);
                return;
            }
        }

        function asyncPostProcessCleanupRows() {
            if (postProcessedCleanupQueue.length > 0) {
                var groupId = postProcessedCleanupQueue[0].groupId;

                // loop through all queue members with this groupID
                while (postProcessedCleanupQueue.length > 0 && postProcessedCleanupQueue[0].groupId == groupId) {
                    var entry = postProcessedCleanupQueue.shift();
                    if (entry.actionType == 'R') {
                        $(entry.node).remove();
                    }
                    if (entry.actionType == 'C') {
                        var column = columns[entry.columnIdx];
                        if (column.asyncPostRenderCleanup && entry.node) {
                            // cleanup must also remove element
                            column.asyncPostRenderCleanup(entry.node, entry.rowIdx, column);
                        }
                    }
                }

                // call this function again after the specified delay
                h_postrenderCleanup = setTimeout(asyncPostProcessCleanupRows, options.asyncPostRenderCleanupDelay);
            }
        }

        function updateCellCssStylesOnRenderedRows(addedHash, removedHash) {
            var node, columnId, addedRowHash, removedRowHash;
            for (var row in rowsCache) {
                removedRowHash = removedHash && removedHash[row];
                addedRowHash = addedHash && addedHash[row];

                if (removedRowHash) {
                    for (columnId in removedRowHash) {
                        if (!addedRowHash || removedRowHash[columnId] != addedRowHash[columnId]) {
                            node = getCellNode(row, getColumnIndex(columnId));
                            if (node) {
                                $(node).removeClass(removedRowHash[columnId]);
                            }
                        }
                    }
                }

                if (addedRowHash) {
                    for (columnId in addedRowHash) {
                        if (!removedRowHash || removedRowHash[columnId] != addedRowHash[columnId]) {
                            node = getCellNode(row, getColumnIndex(columnId));
                            if (node) {
                                $(node).addClass(addedRowHash[columnId]);
                            }
                        }
                    }
                }
            }
        }

        function addCellCssStyles(key, hash) {
            if (cellCssClasses[key]) {
                throw "addCellCssStyles: cell CSS hash with key '" + key + "' already exists.";
            }

            cellCssClasses[key] = hash;
            updateCellCssStylesOnRenderedRows(hash, null);

            trigger(self.onCellCssStylesChanged, { "key": key, "hash": hash, "grid": self });
        }

        function removeCellCssStyles(key) {
            if (!cellCssClasses[key]) {
                return;
            }

            updateCellCssStylesOnRenderedRows(null, cellCssClasses[key]);
            delete cellCssClasses[key];

            trigger(self.onCellCssStylesChanged, { "key": key, "hash": null, "grid": self });
        }

        function setCellCssStyles(key, hash) {
            var prevHash = cellCssClasses[key];

            cellCssClasses[key] = hash;
            updateCellCssStylesOnRenderedRows(hash, prevHash);

            trigger(self.onCellCssStylesChanged, { "key": key, "hash": hash, "grid": self });
        }

        function getCellCssStyles(key) {
            return cellCssClasses[key];
        }

        function flashCell(row, cell, speed) {
            speed = speed || 100;
            if (rowsCache[row]) {
                var $cell = $(getCellNode(row, cell));

                function toggleCellClass(times) {
                    if (!times) {
                        return;
                    }
                    setTimeout(function () {
                        $cell.queue(function () {
                            $cell.toggleClass(options.cellFlashingCssClass).dequeue();
                            toggleCellClass(times - 1);
                        });
                    },
                        speed);
                }

                toggleCellClass(4);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Interactivity

        function handleMouseWheel(e) {
            var rowNode = $(e.target).closest(".slick-row")[0];
            if (rowNode != rowNodeFromLastMouseWheelEvent) {
                if (zombieRowNodeFromLastMouseWheelEvent && zombieRowNodeFromLastMouseWheelEvent != rowNode) {
                    if (options.enableAsyncPostRenderCleanup && zombieRowPostProcessedFromLastMouseWheelEvent) {
                        queuePostProcessedRowForCleanup(zombieRowCacheFromLastMouseWheelEvent,
                          zombieRowPostProcessedFromLastMouseWheelEvent);
                    } else {
                        $canvas[0].removeChild(zombieRowNodeFromLastMouseWheelEvent);
                    }
                    zombieRowNodeFromLastMouseWheelEvent = null;
                    zombieRowCacheFromLastMouseWheelEvent = null;
                    zombieRowPostProcessedFromLastMouseWheelEvent = null;

                    if (options.enableAsyncPostRenderCleanup) { startPostProcessingCleanup(); }
                }
                rowNodeFromLastMouseWheelEvent = rowNode;
            }
        }

        function handleDragInit(e, dd) {
            var cell = getCellFromEvent(e);
            if (!cell || !cellExists(cell.row, cell.cell)) {
                return false;
            }

            var retval = trigger(self.onDragInit, dd, e);
            if (e.isImmediatePropagationStopped()) {
                return retval;
            }

            // if nobody claims to be handling drag'n'drop by stopping immediate propagation,
            // cancel out of it
            return false;
        }

        function handleDragStart(e, dd) {
            var cell = getCellFromEvent(e);
            if (!cell || !cellExists(cell.row, cell.cell)) {
                return false;
            }

            var retval = trigger(self.onDragStart, dd, e);
            if (e.isImmediatePropagationStopped()) {
                return retval;
            }

            return false;
        }

        function handleDrag(e, dd) {
            return trigger(self.onDrag, dd, e);
        }

        function handleDragEnd(e, dd) {
            trigger(self.onDragEnd, dd, e);
        }

        function handleKeyDown(e) {
            trigger(self.onKeyDown, { row: activeRow, cell: activeCell, grid: self }, e);
            var handled = e.isImmediatePropagationStopped();
            var keyCode = Slick.keyCode;

            if (!handled) {
                if (!e.shiftKey && !e.altKey && !e.ctrlKey) {
                    // editor may specify an array of keys to bubble
                    if (options.editable && currentEditor && currentEditor.keyCaptureList) {
                        if (currentEditor.keyCaptureList.indexOf(e.which) > -1) {
                            return;
                        }
                    }
                    if (e.which == keyCode.ESCAPE) {
                        if (!getEditorLock().isActive()) {
                            return; // no editing mode to cancel, allow bubbling and default processing (exit without cancelling the event)
                        }
                        cancelEditAndSetFocus();
                    } else if (e.which == keyCode.PAGE_DOWN) {
                        navigatePageDown();
                        handled = true;
                    } else if (e.which == keyCode.PAGE_UP) {
                        navigatePageUp();
                        handled = true;
                    } else if (e.which == keyCode.LEFT) {
                        handled = navigateLeft();
                    } else if (e.which == keyCode.RIGHT) {
                        handled = navigateRight();
                    } else if (e.which == keyCode.UP) {
                        handled = navigateUp();
                    } else if (e.which == keyCode.DOWN) {
                        handled = navigateDown();
                    } else if (e.which == keyCode.TAB) {
                        handled = navigateNext();
                    } else if (e.which == keyCode.ENTER) {
                        if (options.editable) {
                            if (currentEditor) {
                                // adding new row
                                if (activeRow === getDataLength()) {
                                    navigateDown();
                                } else {
                                    commitEditAndSetFocus();
                                }
                            } else {
                                if (getEditorLock().commitCurrentEdit()) {
                                    makeActiveCellEditable();
                                }
                            }
                        }
                        handled = true;
                    }
                } else if (e.which == keyCode.TAB && e.shiftKey && !e.ctrlKey && !e.altKey) {
                    handled = navigatePrev();
                }
            }

            if (handled) {
                // the event has been handled so don't let parent element (bubbling/propagation) or browser (default) handle it
                e.stopPropagation();
                e.preventDefault();
                try {
                    e.originalEvent.keyCode = 0; // prevent default behaviour for special keys in IE browsers (F3, F5, etc.)
                }
                // ignore exceptions - setting the original event's keycode throws access denied exception for "Ctrl"
                // (hitting control key only, nothing else), "Shift" (maybe others)
                catch (error) {
                }
            }
        }

        function handleClick(e) {
            if (!currentEditor) {
                // if this click resulted in some cell child node getting focus,
                // don't steal it back - keyboard events will still bubble up
                // IE9+ seems to default DIVs to tabIndex=0 instead of -1, so check for cell clicks directly.
                if (e.target != document.activeElement || $(e.target).hasClass("slick-cell")) {
                    setFocus();
                }
            }

            var cell = getCellFromEvent(e);
            if (!cell || (currentEditor !== null && activeRow == cell.row && activeCell == cell.cell)) {
                return;
            }

            trigger(self.onClick, { row: cell.row, cell: cell.cell, grid: self }, e);
            if (e.isImmediatePropagationStopped()) {
                return;
            }

            // this optimisation causes trouble - MLeibman #329
            //if ((activeCell != cell.cell || activeRow != cell.row) && canCellBeActive(cell.row, cell.cell)) {
            if (canCellBeActive(cell.row, cell.cell)) {
                if (!getEditorLock().isActive() || getEditorLock().commitCurrentEdit()) {
                    scrollRowIntoView(cell.row, false);
                    setActiveCellInternal(getCellNode(cell.row, cell.cell));
                }
            }
        }

        function handleContextMenu(e) {
            var $cell = $(e.target).closest(".slick-cell", $canvas);
            if ($cell.length === 0) {
                return;
            }

            // are we editing this cell?
            if (activeCellNode === $cell[0] && currentEditor !== null) {
                return;
            }

            trigger(self.onContextMenu, { grid: self }, e);
        }

        function handleDblClick(e) {
            var cell = getCellFromEvent(e);
            if (!cell || (currentEditor !== null && activeRow == cell.row && activeCell == cell.cell)) {
                return;
            }

            trigger(self.onDblClick, { row: cell.row, cell: cell.cell, grid: self }, e);
            if (e.isImmediatePropagationStopped()) {
                return;
            }

            if (options.editable) {
                gotoCell(cell.row, cell.cell, true);
            }
        }

        function handleHeaderMouseEnter(e) {
            trigger(self.onHeaderMouseEnter, {
                "column": $(this).data("column"),
                "grid": self
            }, e);
        }

        function handleHeaderMouseLeave(e) {
            trigger(self.onHeaderMouseLeave, {
                "column": $(this).data("column"),
                "grid": self
            }, e);
        }

        function handleHeaderContextMenu(e) {
            var $header = $(e.target).closest(".slick-header-column", ".slick-header-columns");
            var column = $header && $header.data("column");
            trigger(self.onHeaderContextMenu, { column: column, grid: self }, e);
        }

        function handleHeaderClick(e) {
            var $header = $(e.target).closest(".slick-header-column", ".slick-header-columns");
            var column = $header && $header.data("column");
            if (column) {
                trigger(self.onHeaderClick, { column: column, grid: self }, e);
            }
        }

        function handleMouseEnter(e) {
            trigger(self.onMouseEnter, { grid: self }, e);
        }

        function handleMouseLeave(e) {
            trigger(self.onMouseLeave, { grid: self }, e);
        }

        function cellExists(row, cell) {
            return !(row < 0 || row >= getDataLength() || cell < 0 || cell >= columns.length);
        }

        function getCellFromPoint(x, y) {
            var row = getRowFromPosition(y);
            var cell = 0;

            var w = 0;
            for (var i = 0; i < columns.length && w < x; i++) {
                w += columns[i].width;
                cell++;
            }

            if (cell < 0) {
                cell = 0;
            }

            return { row: row, cell: cell - 1 };
        }

        function getCellFromNode(cellNode) {
            // read column number from .l<columnNumber> CSS class
            var cls = /l\d+/.exec(cellNode.className);
            if (!cls) {
                throw "getCellFromNode: cannot get cell - " + cellNode.className;
            }
            return parseInt(cls[0].substr(1, cls[0].length - 1), 10);
        }

        function getRowFromNode(rowNode) {
            for (var row in rowsCache) {
                if (rowsCache[row].rowNode === rowNode) {
                    return row | 0;
                }
            }

            return null;
        }

        function getCellFromEvent(e) {
            var $cell = $(e.target).closest(".slick-cell", $canvas);
            if (!$cell.length) {
                return null;
            }

            var row = getRowFromNode($cell[0].parentNode);
            var cell = getCellFromNode($cell[0]);

            if (row == null || cell == null) {
                return null;
            } else {
                return {
                    "row": row,
                    "cell": cell
                };
            }
        }

        function getCellNodeBox(row, cell) {
            if (!cellExists(row, cell)) {
                return null;
            }

            var y1 = getRowTop(row);
            var y2 = y1 + options.rowHeight - 1;
            var x1 = 0;
            for (var i = 0; i < cell; i++) {
                x1 += columns[i].width;
            }
            var x2 = x1 + columns[cell].width;

            return {
                top: y1,
                left: x1,
                bottom: y2,
                right: x2
            };
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Cell switching

        function resetActiveCell() {
            setActiveCellInternal(null, false);
        }

        function setFocus() {
            if (tabbingDirection == -1) {
                $focusSink[0].focus();
            } else {
                $focusSink2[0].focus();
            }
        }

        function scrollCellIntoView(row, cell, doPaging) {
            scrollRowIntoView(row, doPaging);

            var colspan = getColspan(row, cell);
            var left = columnPosLeft[cell],
              right = columnPosRight[cell + (colspan > 1 ? colspan - 1 : 0)],
              scrollRight = scrollLeft + viewportW;

            if (left < scrollLeft) {
                $viewport.scrollLeft(left);
                handleScroll();
                render();
            } else if (right > scrollRight) {
                $viewport.scrollLeft(Math.min(left, right - $viewport[0].clientWidth));
                handleScroll();
                render();
            }
        }

        function setActiveCellInternal(newCell, opt_editMode) {
            if (activeCellNode !== null) {
                makeActiveCellNormal();
                $(activeCellNode).removeClass("active");
                if (rowsCache[activeRow]) {
                    $(rowsCache[activeRow].rowNode).removeClass("active");
                }
            }

            var activeCellChanged = (activeCellNode !== newCell);
            activeCellNode = newCell;

            if (activeCellNode != null) {
                activeRow = getRowFromNode(activeCellNode.parentNode);
                activeCell = activePosX = getCellFromNode(activeCellNode);

                if (opt_editMode == null) {
                    opt_editMode = (activeRow == getDataLength()) || options.autoEdit;
                }

                $(activeCellNode).addClass("active");
                $(rowsCache[activeRow].rowNode).addClass("active");

                if (options.editable && opt_editMode && isCellPotentiallyEditable(activeRow, activeCell)) {
                    clearTimeout(h_editorLoader);

                    if (options.asyncEditorLoading) {
                        h_editorLoader = setTimeout(function () {
                            makeActiveCellEditable();
                        }, options.asyncEditorLoadDelay);
                    } else {
                        makeActiveCellEditable();
                    }
                }
            } else {
                activeRow = activeCell = null;
            }

            // this optimisation causes trouble - MLeibman #329
            //if (activeCellChanged) {
            trigger(self.onActiveCellChanged, getActiveCell());
            //}
        }

        function clearTextSelection() {
            if (document.selection && document.selection.empty) {
                try {
                    //IE fails here if selected element is not in dom
                    document.selection.empty();
                } catch (e) { }
            } else if (window.getSelection) {
                var sel = window.getSelection();
                if (sel && sel.removeAllRanges) {
                    sel.removeAllRanges();
                }
            }
        }

        function isCellPotentiallyEditable(row, cell) {
            var dataLength = getDataLength();
            // is the data for this row loaded?
            if (row < dataLength && !getDataItem(row)) {
                return false;
            }

            // are we in the Add New row?  can we create new from this cell?
            if (columns[cell].cannotTriggerInsert && row >= dataLength) {
                return false;
            }

            // does this cell have an editor?
            if (!getEditor(row, cell)) {
                return false;
            }

            return true;
        }

        function makeActiveCellNormal() {
            if (!currentEditor) {
                return;
            }
            trigger(self.onBeforeCellEditorDestroy, { editor: currentEditor, grid: self });
            currentEditor.destroy();
            currentEditor = null;

            if (activeCellNode) {
                var d = getDataItem(activeRow);
                $(activeCellNode).removeClass("editable invalid");
                if (d) {
                    var column = columns[activeCell];
                    var formatter = getFormatter(activeRow, column);
                    activeCellNode.innerHTML = formatter(activeRow, activeCell, getDataItemValueForColumn(d, column), column, d, self);
                    invalidatePostProcessingResults(activeRow);
                }
            }

            // if there previously was text selected on a page (such as selected text in the edit cell just removed),
            // IE can't set focus to anything else correctly
            if (navigator.userAgent.toLowerCase().match(/msie/)) {
                clearTextSelection();
            }

            getEditorLock().deactivate(editController);
        }

        function makeActiveCellEditable(editor) {
            if (!activeCellNode) {
                return;
            }
            if (!options.editable) {
                throw "Grid : makeActiveCellEditable : should never get called when options.editable is false";
            }

            // cancel pending async call if there is one
            clearTimeout(h_editorLoader);

            if (!isCellPotentiallyEditable(activeRow, activeCell)) {
                return;
            }

            var columnDef = columns[activeCell];
            var item = getDataItem(activeRow);

            if (trigger(self.onBeforeEditCell, { row: activeRow, cell: activeCell, item: item, column: columnDef, grid: self }) === false) {
                setFocus();
                return;
            }

            getEditorLock().activate(editController);
            $(activeCellNode).addClass("editable");

            var useEditor = editor || getEditor(activeRow, activeCell);

            // don't clear the cell if a custom editor is passed through
            if (!editor && !useEditor.suppressClearOnEdit) {
                activeCellNode.innerHTML = "";
            }

            currentEditor = new useEditor({
                grid: self,
                gridPosition: absBox($container[0]),
                position: absBox(activeCellNode),
                container: activeCellNode,
                column: columnDef,
                item: item || {},
                commitChanges: commitEditAndSetFocus,
                cancelChanges: cancelEditAndSetFocus
            });

            if (item) {
                currentEditor.loadValue(item);
            }

            serializedEditorValue = currentEditor.serializeValue();

            if (currentEditor.position) {
                handleActiveCellPositionChange();
            }
        }

        function commitEditAndSetFocus() {
            // if the commit fails, it would do so due to a validation error
            // if so, do not steal the focus from the editor
            if (getEditorLock().commitCurrentEdit()) {
                setFocus();
                if (options.autoEdit) {
                    navigateDown();
                }
            }
        }

        function cancelEditAndSetFocus() {
            if (getEditorLock().cancelCurrentEdit()) {
                setFocus();
            }
        }

        function absBox(elem) {
            var box = {
                top: elem.offsetTop,
                left: elem.offsetLeft,
                bottom: 0,
                right: 0,
                width: $(elem).outerWidth(),
                height: $(elem).outerHeight(),
                visible: true
            };
            box.bottom = box.top + box.height;
            box.right = box.left + box.width;

            // walk up the tree
            var offsetParent = elem.offsetParent;
            while ((elem = elem.parentNode) != document.body) {
                if (elem == null) break;

                if (box.visible && elem.scrollHeight != elem.offsetHeight && $(elem).css("overflowY") != "visible") {
                    box.visible = box.bottom > elem.scrollTop && box.top < elem.scrollTop + elem.clientHeight;
                }

                if (box.visible && elem.scrollWidth != elem.offsetWidth && $(elem).css("overflowX") != "visible") {
                    box.visible = box.right > elem.scrollLeft && box.left < elem.scrollLeft + elem.clientWidth;
                }

                box.left -= elem.scrollLeft;
                box.top -= elem.scrollTop;

                if (elem === offsetParent) {
                    box.left += elem.offsetLeft;
                    box.top += elem.offsetTop;
                    offsetParent = elem.offsetParent;
                }

                box.bottom = box.top + box.height;
                box.right = box.left + box.width;
            }

            return box;
        }

        function getActiveCellPosition() {
            return absBox(activeCellNode);
        }

        function getGridPosition() {
            return absBox($container[0])
        }

        function handleActiveCellPositionChange() {
            if (!activeCellNode) {
                return;
            }

            trigger(self.onActiveCellPositionChanged, { grid: self });

            if (currentEditor) {
                var cellBox = getActiveCellPosition();
                if (currentEditor.show && currentEditor.hide) {
                    if (!cellBox.visible) {
                        currentEditor.hide();
                    } else {
                        currentEditor.show();
                    }
                }

                if (currentEditor.position) {
                    currentEditor.position(cellBox);
                }
            }
        }

        function getCellEditor() {
            return currentEditor;
        }

        function getActiveCell() {
            if (!activeCellNode) {
                return null;
            } else {
                return { row: activeRow, cell: activeCell, grid: self };
            }
        }

        function getActiveCellNode() {
            return activeCellNode;
        }

        function scrollRowIntoView(row, doPaging) {
            var rowAtTop = row * options.rowHeight;
            var rowAtBottom = (row + 1) * options.rowHeight - viewportH + (viewportHasHScroll ? scrollbarDimensions.height : 0);

            // need to page down?
            if ((row + 1) * options.rowHeight > scrollTop + viewportH + offset) {
                scrollTo(doPaging ? rowAtTop : rowAtBottom);
                render();
            }
                // or page up?
            else if (row * options.rowHeight < scrollTop + offset) {
                scrollTo(doPaging ? rowAtBottom : rowAtTop);
                render();
            }
        }

        function scrollRowToTop(row) {
            scrollTo(row * options.rowHeight);
            render();
        }

        function scrollPage(dir) {
            var deltaRows = dir * numVisibleRows;
            scrollTo((getRowFromPosition(scrollTop) + deltaRows) * options.rowHeight);
            render();

            if (options.enableCellNavigation && activeRow != null) {
                var row = activeRow + deltaRows;
                var dataLengthIncludingAddNew = getDataLengthIncludingAddNew();
                if (row >= dataLengthIncludingAddNew) {
                    row = dataLengthIncludingAddNew - 1;
                }
                if (row < 0) {
                    row = 0;
                }

                var cell = 0, prevCell = null;
                var prevActivePosX = activePosX;
                while (cell <= activePosX) {
                    if (canCellBeActive(row, cell)) {
                        prevCell = cell;
                    }
                    cell += getColspan(row, cell);
                }

                if (prevCell !== null) {
                    setActiveCellInternal(getCellNode(row, prevCell));
                    activePosX = prevActivePosX;
                } else {
                    resetActiveCell();
                }
            }
        }

        function navigatePageDown() {
            scrollPage(1);
        }

        function navigatePageUp() {
            scrollPage(-1);
        }

        function getColspan(row, cell) {
            var metadata = data.getItemMetadata && data.getItemMetadata(row);
            if (!metadata || !metadata.columns) {
                return 1;
            }

            var columnData = metadata.columns[columns[cell].id] || metadata.columns[cell];
            var colspan = (columnData && columnData.colspan);
            if (colspan === "*") {
                colspan = columns.length - cell;
            } else {
                colspan = colspan || 1;
            }

            return colspan;
        }

        function findFirstFocusableCell(row) {
            var cell = 0;
            while (cell < columns.length) {
                if (canCellBeActive(row, cell)) {
                    return cell;
                }
                cell += getColspan(row, cell);
            }
            return null;
        }

        function findLastFocusableCell(row) {
            var cell = 0;
            var lastFocusableCell = null;
            while (cell < columns.length) {
                if (canCellBeActive(row, cell)) {
                    lastFocusableCell = cell;
                }
                cell += getColspan(row, cell);
            }
            return lastFocusableCell;
        }

        function gotoRight(row, cell, posX) {
            if (cell >= columns.length) {
                return null;
            }

            do {
                cell += getColspan(row, cell);
            }
            while (cell < columns.length && !canCellBeActive(row, cell));

            if (cell < columns.length) {
                return {
                    "row": row,
                    "cell": cell,
                    "posX": cell
                };
            }
            return null;
        }

        function gotoLeft(row, cell, posX) {
            if (cell <= 0) {
                return null;
            }

            var firstFocusableCell = findFirstFocusableCell(row);
            if (firstFocusableCell === null || firstFocusableCell >= cell) {
                return null;
            }

            var prev = {
                "row": row,
                "cell": firstFocusableCell,
                "posX": firstFocusableCell
            };
            var pos;
            while (true) {
                pos = gotoRight(prev.row, prev.cell, prev.posX);
                if (!pos) {
                    return null;
                }
                if (pos.cell >= cell) {
                    return prev;
                }
                prev = pos;
            }
        }

        function gotoDown(row, cell, posX) {
            var prevCell;
            var dataLengthIncludingAddNew = getDataLengthIncludingAddNew();
            while (true) {
                if (++row >= dataLengthIncludingAddNew) {
                    return null;
                }

                prevCell = cell = 0;
                while (cell <= posX) {
                    prevCell = cell;
                    cell += getColspan(row, cell);
                }

                if (canCellBeActive(row, prevCell)) {
                    return {
                        "row": row,
                        "cell": prevCell,
                        "posX": posX
                    };
                }
            }
        }

        function gotoUp(row, cell, posX) {
            var prevCell;
            while (true) {
                if (--row < 0) {
                    return null;
                }

                prevCell = cell = 0;
                while (cell <= posX) {
                    prevCell = cell;
                    cell += getColspan(row, cell);
                }

                if (canCellBeActive(row, prevCell)) {
                    return {
                        "row": row,
                        "cell": prevCell,
                        "posX": posX
                    };
                }
            }
        }

        function gotoNext(row, cell, posX) {
            if (row == null && cell == null) {
                row = cell = posX = 0;
                if (canCellBeActive(row, cell)) {
                    return {
                        "row": row,
                        "cell": cell,
                        "posX": cell
                    };
                }
            }

            var pos = gotoRight(row, cell, posX);
            if (pos) {
                return pos;
            }

            var firstFocusableCell = null;
            var dataLengthIncludingAddNew = getDataLengthIncludingAddNew();
            while (++row < dataLengthIncludingAddNew) {
                firstFocusableCell = findFirstFocusableCell(row);
                if (firstFocusableCell !== null) {
                    return {
                        "row": row,
                        "cell": firstFocusableCell,
                        "posX": firstFocusableCell
                    };
                }
            }
            return null;
        }

        function gotoPrev(row, cell, posX) {
            if (row == null && cell == null) {
                row = getDataLengthIncludingAddNew() - 1;
                cell = posX = columns.length - 1;
                if (canCellBeActive(row, cell)) {
                    return {
                        "row": row,
                        "cell": cell,
                        "posX": cell
                    };
                }
            }

            var pos;
            var lastSelectableCell;
            while (!pos) {
                pos = gotoLeft(row, cell, posX);
                if (pos) {
                    break;
                }
                if (--row < 0) {
                    return null;
                }

                cell = 0;
                lastSelectableCell = findLastFocusableCell(row);
                if (lastSelectableCell !== null) {
                    pos = {
                        "row": row,
                        "cell": lastSelectableCell,
                        "posX": lastSelectableCell
                    };
                }
            }
            return pos;
        }

        function navigateRight() {
            return navigate("right");
        }

        function navigateLeft() {
            return navigate("left");
        }

        function navigateDown() {
            return navigate("down");
        }

        function navigateUp() {
            return navigate("up");
        }

        function navigateNext() {
            return navigate("next");
        }

        function navigatePrev() {
            return navigate("prev");
        }

        /**
         * @param {string} dir Navigation direction.
         * @return {boolean} Whether navigation resulted in a change of active cell.
         */
        function navigate(dir) {
            if (!options.enableCellNavigation) {
                return false;
            }

            if (!activeCellNode && dir != "prev" && dir != "next") {
                return false;
            }

            if (!getEditorLock().commitCurrentEdit()) {
                return true;
            }
            setFocus();

            var tabbingDirections = {
                "up": -1,
                "down": 1,
                "left": -1,
                "right": 1,
                "prev": -1,
                "next": 1
            };
            tabbingDirection = tabbingDirections[dir];

            var stepFunctions = {
                "up": gotoUp,
                "down": gotoDown,
                "left": gotoLeft,
                "right": gotoRight,
                "prev": gotoPrev,
                "next": gotoNext
            };
            var stepFn = stepFunctions[dir];
            var pos = stepFn(activeRow, activeCell, activePosX);
            if (pos) {
                var isAddNewRow = (pos.row == getDataLength());
                scrollCellIntoView(pos.row, pos.cell, !isAddNewRow);
                setActiveCellInternal(getCellNode(pos.row, pos.cell));
                activePosX = pos.posX;
                return true;
            } else {
                setActiveCellInternal(getCellNode(activeRow, activeCell));
                return false;
            }
        }

        function getCellNode(row, cell) {
            if (rowsCache[row]) {
                ensureCellNodesInRowsCache(row);
                return rowsCache[row].cellNodesByColumnIdx[cell];
            }
            return null;
        }

        function setActiveCell(row, cell) {
            if (!initialized) { return; }
            if (row > getDataLength() || row < 0 || cell >= columns.length || cell < 0) {
                return;
            }

            if (!options.enableCellNavigation) {
                return;
            }

            scrollCellIntoView(row, cell, false);
            setActiveCellInternal(getCellNode(row, cell), false);
        }

        function canCellBeActive(row, cell) {
            if (!options.enableCellNavigation || row >= getDataLengthIncludingAddNew() ||
                row < 0 || cell >= columns.length || cell < 0) {
                return false;
            }

            var rowMetadata = data.getItemMetadata && data.getItemMetadata(row);
            if (rowMetadata && typeof rowMetadata.focusable === "boolean") {
                return rowMetadata.focusable;
            }

            var columnMetadata = rowMetadata && rowMetadata.columns;
            if (columnMetadata && columnMetadata[columns[cell].id] && typeof columnMetadata[columns[cell].id].focusable === "boolean") {
                return columnMetadata[columns[cell].id].focusable;
            }
            if (columnMetadata && columnMetadata[cell] && typeof columnMetadata[cell].focusable === "boolean") {
                return columnMetadata[cell].focusable;
            }

            return columns[cell].focusable;
        }

        function canCellBeSelected(row, cell) {
            if (row >= getDataLength() || row < 0 || cell >= columns.length || cell < 0) {
                return false;
            }

            var rowMetadata = data.getItemMetadata && data.getItemMetadata(row);
            if (rowMetadata && typeof rowMetadata.selectable === "boolean") {
                return rowMetadata.selectable;
            }

            var columnMetadata = rowMetadata && rowMetadata.columns && (rowMetadata.columns[columns[cell].id] || rowMetadata.columns[cell]);
            if (columnMetadata && typeof columnMetadata.selectable === "boolean") {
                return columnMetadata.selectable;
            }

            return columns[cell].selectable;
        }

        function gotoCell(row, cell, forceEdit) {
            if (!initialized) { return; }
            if (!canCellBeActive(row, cell)) {
                return;
            }

            if (!getEditorLock().commitCurrentEdit()) {
                return;
            }

            scrollCellIntoView(row, cell, false);

            var newCell = getCellNode(row, cell);

            // if selecting the 'add new' row, start editing right away
            setActiveCellInternal(newCell, forceEdit || (row === getDataLength()) || options.autoEdit);

            // if no editor was created, set the focus back on the grid
            if (!currentEditor) {
                setFocus();
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////////////
        // IEditor implementation for the editor lock

        function commitCurrentEdit() {
            var item = getDataItem(activeRow);
            var column = columns[activeCell];

            if (currentEditor) {
                if (currentEditor.isValueChanged()) {
                    var validationResults = currentEditor.validate();

                    if (validationResults.valid) {
                        if (activeRow < getDataLength()) {
                            var editCommand = {
                                row: activeRow,
                                cell: activeCell,
                                editor: currentEditor,
                                serializedValue: currentEditor.serializeValue(),
                                prevSerializedValue: serializedEditorValue,
                                execute: function () {
                                    this.editor.applyValue(item, this.serializedValue);
                                    updateRow(this.row);
                                    trigger(self.onCellChange, {
                                        row: activeRow,
                                        cell: activeCell,
                                        item: item,
                                        grid: self
                                    });
                                },
                                undo: function () {
                                    this.editor.applyValue(item, this.prevSerializedValue);
                                    updateRow(this.row);
                                    trigger(self.onCellChange, {
                                        row: activeRow,
                                        cell: activeCell,
                                        item: item,
                                        grid: self
                                    });
                                }
                            };

                            if (options.editCommandHandler) {
                                makeActiveCellNormal();
                                options.editCommandHandler(item, column, editCommand);
                            } else {
                                editCommand.execute();
                                makeActiveCellNormal();
                            }

                        } else {
                            var newItem = {};
                            currentEditor.applyValue(newItem, currentEditor.serializeValue());
                            makeActiveCellNormal();
                            trigger(self.onAddNewRow, { item: newItem, column: column, grid: self });
                        }

                        // check whether the lock has been re-acquired by event handlers
                        return !getEditorLock().isActive();
                    } else {
                        // Re-add the CSS class to trigger transitions, if any.
                        $(activeCellNode).removeClass("invalid");
                        $(activeCellNode).width();  // force layout
                        $(activeCellNode).addClass("invalid");

                        trigger(self.onValidationError, {
                            editor: currentEditor,
                            cellNode: activeCellNode,
                            validationResults: validationResults,
                            row: activeRow,
                            cell: activeCell,
                            column: column,
                            grid: self
                        });

                        currentEditor.focus();
                        return false;
                    }
                }

                makeActiveCellNormal();
            }
            return true;
        }

        function cancelCurrentEdit() {
            makeActiveCellNormal();
            return true;
        }

        function rowsToRanges(rows) {
            var ranges = [];
            var lastCell = columns.length - 1;
            for (var i = 0; i < rows.length; i++) {
                ranges.push(new Slick.Range(rows[i], 0, rows[i], lastCell));
            }
            return ranges;
        }

        function getSelectedRows() {
            if (!selectionModel) {
                throw "Selection model is not set";
            }
            return selectedRows;
        }

        function setSelectedRows(rows) {
            if (!selectionModel) {
                throw "Selection model is not set";
            }
            selectionModel.setSelectedRanges(rowsToRanges(rows));
        }


        //////////////////////////////////////////////////////////////////////////////////////////////
        // Debug

        this.debug = function () {
            var s = "";

            s += ("\n" + "counter_rows_rendered:  " + counter_rows_rendered);
            s += ("\n" + "counter_rows_removed:  " + counter_rows_removed);
            s += ("\n" + "renderedRows:  " + renderedRows);
            s += ("\n" + "numVisibleRows:  " + numVisibleRows);
            s += ("\n" + "maxSupportedCssHeight:  " + maxSupportedCssHeight);
            s += ("\n" + "n(umber of pages):  " + n);
            s += ("\n" + "(current) page:  " + page);
            s += ("\n" + "page height (ph):  " + ph);
            s += ("\n" + "vScrollDir:  " + vScrollDir);

            alert(s);
        };

        // a debug helper to be able to access private members
        this.eval = function (expr) {
            return eval(expr);
        };

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Public API

        $.extend(this, {
            "slickGridVersion": "2.3.0",

            // Events
            "onScroll": new Slick.Event(),
            "onSort": new Slick.Event(),
            "onHeaderMouseEnter": new Slick.Event(),
            "onHeaderMouseLeave": new Slick.Event(),
            "onHeaderContextMenu": new Slick.Event(),
            "onHeaderClick": new Slick.Event(),
            "onHeaderCellRendered": new Slick.Event(),
            "onBeforeHeaderCellDestroy": new Slick.Event(),
            "onHeaderRowCellRendered": new Slick.Event(),
            "onFooterRowCellRendered": new Slick.Event(),
            "onBeforeHeaderRowCellDestroy": new Slick.Event(),
            "onBeforeFooterRowCellDestroy": new Slick.Event(),
            "onMouseEnter": new Slick.Event(),
            "onMouseLeave": new Slick.Event(),
            "onClick": new Slick.Event(),
            "onDblClick": new Slick.Event(),
            "onContextMenu": new Slick.Event(),
            "onKeyDown": new Slick.Event(),
            "onAddNewRow": new Slick.Event(),
            "onValidationError": new Slick.Event(),
            "onViewportChanged": new Slick.Event(),
            "onColumnsReordered": new Slick.Event(),
            "onColumnsResized": new Slick.Event(),
            "onCellChange": new Slick.Event(),
            "onBeforeEditCell": new Slick.Event(),
            "onBeforeCellEditorDestroy": new Slick.Event(),
            "onBeforeDestroy": new Slick.Event(),
            "onActiveCellChanged": new Slick.Event(),
            "onActiveCellPositionChanged": new Slick.Event(),
            "onDragInit": new Slick.Event(),
            "onDragStart": new Slick.Event(),
            "onDrag": new Slick.Event(),
            "onDragEnd": new Slick.Event(),
            "onSelectedRowsChanged": new Slick.Event(),
            "onCellCssStylesChanged": new Slick.Event(),

            // Methods
            "registerPlugin": registerPlugin,
            "unregisterPlugin": unregisterPlugin,
            "getColumns": getColumns,
            "setColumns": setColumns,
            "getColumnIndex": getColumnIndex,
            "updateColumnHeader": updateColumnHeader,
            "setSortColumn": setSortColumn,
            "setSortColumns": setSortColumns,
            "getSortColumns": getSortColumns,
            "autosizeColumns": autosizeColumns,
            "getOptions": getOptions,
            "setOptions": setOptions,
            "getData": getData,
            "getDataLength": getDataLength,
            "getDataItem": getDataItem,
            "setData": setData,
            "getSelectionModel": getSelectionModel,
            "setSelectionModel": setSelectionModel,
            "getSelectedRows": getSelectedRows,
            "setSelectedRows": setSelectedRows,
            "getContainerNode": getContainerNode,
            "updatePagingStatusFromView": updatePagingStatusFromView,

            "render": render,
            "invalidate": invalidate,
            "invalidateRow": invalidateRow,
            "invalidateRows": invalidateRows,
            "invalidateAllRows": invalidateAllRows,
            "updateCell": updateCell,
            "updateRow": updateRow,
            "getViewport": getVisibleRange,
            "getRenderedRange": getRenderedRange,
            "resizeCanvas": resizeCanvas,
            "updateRowCount": updateRowCount,
            "scrollRowIntoView": scrollRowIntoView,
            "scrollRowToTop": scrollRowToTop,
            "scrollCellIntoView": scrollCellIntoView,
            "getCanvasNode": getCanvasNode,
            "focus": setFocus,

            "getCellFromPoint": getCellFromPoint,
            "getCellFromEvent": getCellFromEvent,
            "getActiveCell": getActiveCell,
            "setActiveCell": setActiveCell,
            "getActiveCellNode": getActiveCellNode,
            "getActiveCellPosition": getActiveCellPosition,
            "resetActiveCell": resetActiveCell,
            "editActiveCell": makeActiveCellEditable,
            "getCellEditor": getCellEditor,
            "getCellNode": getCellNode,
            "getCellNodeBox": getCellNodeBox,
            "canCellBeSelected": canCellBeSelected,
            "canCellBeActive": canCellBeActive,
            "navigatePrev": navigatePrev,
            "navigateNext": navigateNext,
            "navigateUp": navigateUp,
            "navigateDown": navigateDown,
            "navigateLeft": navigateLeft,
            "navigateRight": navigateRight,
            "navigatePageUp": navigatePageUp,
            "navigatePageDown": navigatePageDown,
            "gotoCell": gotoCell,
            "getTopPanel": getTopPanel,
            "setTopPanelVisibility": setTopPanelVisibility,
            "setHeaderRowVisibility": setHeaderRowVisibility,
            "getHeaderRow": getHeaderRow,
            "getHeaderRowColumn": getHeaderRowColumn,
            "setFooterRowVisibility": setFooterRowVisibility,
            "getFooterRow": getFooterRow,
            "getFooterRowColumn": getFooterRowColumn,
            "getGridPosition": getGridPosition,
            "flashCell": flashCell,
            "addCellCssStyles": addCellCssStyles,
            "setCellCssStyles": setCellCssStyles,
            "removeCellCssStyles": removeCellCssStyles,
            "getCellCssStyles": getCellCssStyles,

            "init": finishInitialization,
            "destroy": destroy,

            // IEditor implementation
            "getEditorLock": getEditorLock,
            "getEditController": getEditController
        });

        init();
    }
}(jQuery));

//#endregion End slick.grid

//#region slick.formatters

/***
 * Contains basic SlickGrid formatters.
 * 
 * NOTE:  These are merely examples.  You will most likely need to implement something more
 *        robust/extensible/localizable/etc. for your use!
 * 
 * @module Formatters
 * @namespace Slick
 */

(function ($) {
    // register namespace
    $.extend(true, window, {
        "Slick": {
            "Formatters": {
                "PercentComplete": PercentCompleteFormatter,
                "PercentCompleteBar": PercentCompleteBarFormatter,
                "YesNo": YesNoFormatter,
                "Checkmark": CheckmarkFormatter
            }
        }
    });

    function PercentCompleteFormatter(row, cell, value, columnDef, dataContext) {
        if (value == null || value === "") {
            return "-";
        } else if (value < 50) {
            return "<span style='color:red;font-weight:bold;'>" + value + "%</span>";
        } else {
            return "<span style='color:green'>" + value + "%</span>";
        }
    }

    function PercentCompleteBarFormatter(row, cell, value, columnDef, dataContext) {
        if (value == null || value === "") {
            return "";
        }

        var color;

        if (value < 30) {
            color = "red";
        } else if (value < 70) {
            color = "silver";
        } else {
            color = "green";
        }

        return "<span class='percent-complete-bar' style='background:" + color + ";width:" + value + "%'></span>";
    }

    function YesNoFormatter(row, cell, value, columnDef, dataContext) {
        return value ? "Yes" : "No";
    }

    function CheckmarkFormatter(row, cell, value, columnDef, dataContext) {
        return value ? "<img src='../images/tick.png'>" : "";
    }
})(jQuery);

//#endregion End slick.formatters

//#region slick.editors

/***
 * Contains basic SlickGrid editors.
 * @module Editors
 * @namespace Slick
 */

(function ($) {
    // register namespace
    $.extend(true, window, {
        "Slick": {
            "Editors": {
                "Text": TextEditor,
                "Integer": IntegerEditor,
                "Float": FloatEditor,
                "Date": DateEditor,
                "YesNoSelect": YesNoSelectEditor,
                "Checkbox": CheckboxEditor,
                "PercentComplete": PercentCompleteEditor,
                "LongText": LongTextEditor
            }
        }
    });

    function TextEditor(args) {
        var $input;
        var defaultValue;
        var scope = this;

        this.init = function () {
            $input = $("<INPUT type=text class='editor-text' />")
                .appendTo(args.container)
                .on("keydown.nav", function (e) {
                    if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                        e.stopImmediatePropagation();
                    }
                })
                .focus()
                .select();
        };

        this.destroy = function () {
            $input.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.getValue = function () {
            return $input.val();
        };

        this.setValue = function (val) {
            $input.val(val);
        };

        this.loadValue = function (item) {
            defaultValue = item[args.column.field] || "";
            $input.val(defaultValue);
            $input[0].defaultValue = defaultValue;
            $input.select();
        };

        this.serializeValue = function () {
            return $input.val();
        };

        this.applyValue = function (item, state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function () {
            if (args.column.validator) {
                var validationResults = args.column.validator($input.val());
                if (!validationResults.valid) {
                    return validationResults;
                }
            }

            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function IntegerEditor(args) {
        var $input;
        var defaultValue;
        var scope = this;

        this.init = function () {
            $input = $("<INPUT type=text class='editor-text' />");

            $input.on("keydown.nav", function (e) {
                if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                    e.stopImmediatePropagation();
                }
            });

            $input.appendTo(args.container);
            $input.focus().select();
        };

        this.destroy = function () {
            $input.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            defaultValue = item[args.column.field];
            $input.val(defaultValue);
            $input[0].defaultValue = defaultValue;
            $input.select();
        };

        this.serializeValue = function () {
            return parseInt($input.val(), 10) || 0;
        };

        this.applyValue = function (item, state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function () {
            if (isNaN($input.val())) {
                return {
                    valid: false,
                    msg: "Please enter a valid integer"
                };
            }

            if (args.column.validator) {
                var validationResults = args.column.validator($input.val());
                if (!validationResults.valid) {
                    return validationResults;
                }
            }

            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function FloatEditor(args) {
        var $input;
        var defaultValue;
        var scope = this;

        this.init = function () {
            $input = $("<INPUT type=text class='editor-text' />");

            $input.on("keydown.nav", function (e) {
                if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                    e.stopImmediatePropagation();
                }
            });

            $input.appendTo(args.container);
            $input.focus().select();
        };

        this.destroy = function () {
            $input.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        function getDecimalPlaces() {
            // returns the number of fixed decimal places or null
            var rtn = args.column.editorFixedDecimalPlaces;
            if (typeof rtn == 'undefined') {
                rtn = FloatEditor.DefaultDecimalPlaces;
            }
            return (!rtn && rtn !== 0 ? null : rtn);
        }

        this.loadValue = function (item) {
            defaultValue = item[args.column.field];

            var decPlaces = getDecimalPlaces();
            if (decPlaces !== null
            && (defaultValue || defaultValue === 0)
            && defaultValue.toFixed) {
                defaultValue = defaultValue.toFixed(decPlaces);
            }

            $input.val(defaultValue);
            $input[0].defaultValue = defaultValue;
            $input.select();
        };

        this.serializeValue = function () {
            var rtn = parseFloat($input.val()) || 0;

            var decPlaces = getDecimalPlaces();
            if (decPlaces !== null
            && (rtn || rtn === 0)
            && rtn.toFixed) {
                rtn = parseFloat(rtn.toFixed(decPlaces));
            }

            return rtn;
        };

        this.applyValue = function (item, state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function () {
            if (isNaN($input.val())) {
                return {
                    valid: false,
                    msg: "Please enter a valid number"
                };
            }

            if (args.column.validator) {
                var validationResults = args.column.validator($input.val());
                if (!validationResults.valid) {
                    return validationResults;
                }
            }

            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    FloatEditor.DefaultDecimalPlaces = null;

    function DateEditor(args) {
        var $input;
        var defaultValue;
        var scope = this;
        var calendarOpen = false;

        this.init = function () {
            $input = $("<INPUT type=text class='editor-text' />");
            $input.appendTo(args.container);
            $input.focus().select();
            $input.datepicker({
                showOn: "button",
                buttonImageOnly: true,
                buttonImage: "../images/calendar.gif",
                beforeShow: function () {
                    calendarOpen = true
                },
                onClose: function () {
                    calendarOpen = false
                }
            });
            $input.width($input.width() - 18);
        };

        this.destroy = function () {
            $.datepicker.dpDiv.stop(true, true);
            $input.datepicker("hide");
            $input.datepicker("destroy");
            $input.remove();
        };

        this.show = function () {
            if (calendarOpen) {
                $.datepicker.dpDiv.stop(true, true).show();
            }
        };

        this.hide = function () {
            if (calendarOpen) {
                $.datepicker.dpDiv.stop(true, true).hide();
            }
        };

        this.position = function (position) {
            if (!calendarOpen) {
                return;
            }
            $.datepicker.dpDiv
                .css("top", position.top + 30)
                .css("left", position.left);
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            defaultValue = item[args.column.field];
            $input.val(defaultValue);
            $input[0].defaultValue = defaultValue;
            $input.select();
        };

        this.serializeValue = function () {
            return $input.val();
        };

        this.applyValue = function (item, state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function () {
            if (args.column.validator) {
                var validationResults = args.column.validator($input.val());
                if (!validationResults.valid) {
                    return validationResults;
                }
            }

            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function YesNoSelectEditor(args) {
        var $select;
        var defaultValue;
        var scope = this;

        this.init = function () {
            $select = $("<SELECT tabIndex='0' class='editor-yesno'><OPTION value='yes'>Yes</OPTION><OPTION value='no'>No</OPTION></SELECT>");
            $select.appendTo(args.container);
            $select.focus();
        };

        this.destroy = function () {
            $select.remove();
        };

        this.focus = function () {
            $select.focus();
        };

        this.loadValue = function (item) {
            $select.val((defaultValue = item[args.column.field]) ? "yes" : "no");
            $select.select();
        };

        this.serializeValue = function () {
            return ($select.val() == "yes");
        };

        this.applyValue = function (item, state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return ($select.val() != defaultValue);
        };

        this.validate = function () {
            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function CheckboxEditor(args) {
        var $select;
        var defaultValue;
        var scope = this;

        this.init = function () {
            $select = $("<INPUT type=checkbox value='true' class='editor-checkbox' hideFocus>");
            $select.appendTo(args.container);
            $select.focus();
        };

        this.destroy = function () {
            $select.remove();
        };

        this.focus = function () {
            $select.focus();
        };

        this.loadValue = function (item) {
            defaultValue = !!item[args.column.field];
            if (defaultValue) {
                $select.prop('checked', true);
            } else {
                $select.prop('checked', false);
            }
        };

        this.serializeValue = function () {
            return $select.prop('checked');
        };

        this.applyValue = function (item, state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return (this.serializeValue() !== defaultValue);
        };

        this.validate = function () {
            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    function PercentCompleteEditor(args) {
        var $input, $picker;
        var defaultValue;
        var scope = this;

        this.init = function () {
            $input = $("<INPUT type=text class='editor-percentcomplete' />");
            $input.width($(args.container).innerWidth() - 25);
            $input.appendTo(args.container);

            $picker = $("<div class='editor-percentcomplete-picker' />").appendTo(args.container);
            $picker.append("<div class='editor-percentcomplete-helper'><div class='editor-percentcomplete-wrapper'><div class='editor-percentcomplete-slider' /><div class='editor-percentcomplete-buttons' /></div></div>");

            $picker.find(".editor-percentcomplete-buttons").append("<button val=0>Not started</button><br/><button val=50>In Progress</button><br/><button val=100>Complete</button>");

            $input.focus().select();

            $picker.find(".editor-percentcomplete-slider").slider({
                orientation: "vertical",
                range: "min",
                value: defaultValue,
                slide: function (event, ui) {
                    $input.val(ui.value)
                }
            });

            $picker.find(".editor-percentcomplete-buttons button").on("click", function (e) {
                $input.val($(this).attr("val"));
                $picker.find(".editor-percentcomplete-slider").slider("value", $(this).attr("val"));
            })
        };

        this.destroy = function () {
            $input.remove();
            $picker.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            $input.val(defaultValue = item[args.column.field]);
            $input.select();
        };

        this.serializeValue = function () {
            return parseInt($input.val(), 10) || 0;
        };

        this.applyValue = function (item, state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ((parseInt($input.val(), 10) || 0) != defaultValue);
        };

        this.validate = function () {
            if (isNaN(parseInt($input.val(), 10))) {
                return {
                    valid: false,
                    msg: "Please enter a valid positive number"
                };
            }

            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }

    /*
     * An example of a "detached" editor.
     * The UI is added onto document BODY and .position(), .show() and .hide() are implemented.
     * KeyDown events are also handled to provide handling for Tab, Shift-Tab, Esc and Ctrl-Enter.
     */
    function LongTextEditor(args) {
        var $input, $wrapper;
        var defaultValue;
        var scope = this;

        this.init = function () {
            var $container = $("body");

            $wrapper = $("<DIV style='z-index:10000;position:absolute;background:white;padding:5px;border:3px solid gray; -moz-border-radius:10px; border-radius:10px;'/>")
                .appendTo($container);

            $input = $("<TEXTAREA hidefocus rows=5 style='backround:white;width:250px;height:80px;border:0;outline:0'>")
                .appendTo($wrapper);

            $("<DIV style='text-align:right'><BUTTON>Save</BUTTON><BUTTON>Cancel</BUTTON></DIV>")
                .appendTo($wrapper);

            $wrapper.find("button:first").on("click", this.save);
            $wrapper.find("button:last").on("click", this.cancel);
            $input.on("keydown", this.handleKeyDown);

            scope.position(args.position);
            $input.focus().select();
        };

        this.handleKeyDown = function (e) {
            if (e.which == $.ui.keyCode.ENTER && e.ctrlKey) {
                scope.save();
            } else if (e.which == $.ui.keyCode.ESCAPE) {
                e.preventDefault();
                scope.cancel();
            } else if (e.which == $.ui.keyCode.TAB && e.shiftKey) {
                e.preventDefault();
                args.grid.navigatePrev();
            } else if (e.which == $.ui.keyCode.TAB) {
                e.preventDefault();
                args.grid.navigateNext();
            }
        };

        this.save = function () {
            args.commitChanges();
        };

        this.cancel = function () {
            $input.val(defaultValue);
            args.cancelChanges();
        };

        this.hide = function () {
            $wrapper.hide();
        };

        this.show = function () {
            $wrapper.show();
        };

        this.position = function (position) {
            $wrapper
                .css("top", position.top - 5)
                .css("left", position.left - 5)
        };

        this.destroy = function () {
            $wrapper.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            $input.val(defaultValue = item[args.column.field]);
            $input.select();
        };

        this.serializeValue = function () {
            return $input.val();
        };

        this.applyValue = function (item, state) {
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
        };

        this.validate = function () {
            if (args.column.validator) {
                var validationResults = args.column.validator($input.val());
                if (!validationResults.valid) {
                    return validationResults;
                }
            }

            return {
                valid: true,
                msg: null
            };
        };

        this.init();
    }
})(jQuery);

//#endregion End slick.editors

//#region plugins/RowMoveManager

(function ($) {
    // register namespace
    $.extend(true, window, {
        "Slick": {
            "RowMoveManager": RowMoveManager
        }
    });

    function RowMoveManager(options) {
        var _grid;
        var _canvas;
        var _dragging;
        var _self = this;
        var _handler = new Slick.EventHandler();
        var _defaults = {
            cancelEditOnDrag: false
        };

        function init(grid) {
            options = $.extend(true, {}, _defaults, options);
            _grid = grid;
            _canvas = _grid.getCanvasNode();
            _handler
              .subscribe(_grid.onDragInit, handleDragInit)
              .subscribe(_grid.onDragStart, handleDragStart)
              .subscribe(_grid.onDrag, handleDrag)
              .subscribe(_grid.onDragEnd, handleDragEnd);
        }

        function destroy() {
            _handler.unsubscribeAll();
        }

        function handleDragInit(e, dd) {
            // prevent the grid from cancelling drag'n'drop by default
            e.stopImmediatePropagation();
        }

        function handleDragStart(e, dd) {
            var cell = _grid.getCellFromEvent(e);

            if (options.cancelEditOnDrag && _grid.getEditorLock().isActive()) {
                _grid.getEditorLock().cancelCurrentEdit();
            }

            if (_grid.getEditorLock().isActive() || !/move|selectAndMove/.test(_grid.getColumns()[cell.cell].behavior)) {
                return false;
            }

            _dragging = true;
            e.stopImmediatePropagation();

            var selectedRows = _grid.getSelectedRows();

            if (selectedRows.length == 0 || $.inArray(cell.row, selectedRows) == -1) {
                selectedRows = [cell.row];
                _grid.setSelectedRows(selectedRows);
            }

            var rowHeight = _grid.getOptions().rowHeight;

            dd.selectedRows = selectedRows;

            dd.selectionProxy = $("<div class='slick-reorder-proxy'/>")
                .css("position", "absolute")
                .css("zIndex", "99999")
                .css("width", $(_canvas).innerWidth())
                .css("height", rowHeight * selectedRows.length)
                .appendTo(_canvas);

            dd.guide = $("<div class='slick-reorder-guide'/>")
                .css("position", "absolute")
                .css("zIndex", "99998")
                .css("width", $(_canvas).innerWidth())
                .css("top", -1000)
                .appendTo(_canvas);

            dd.insertBefore = -1;
        }

        function handleDrag(e, dd) {
            if (!_dragging) {
                return;
            }

            e.stopImmediatePropagation();

            var top = e.pageY - $(_canvas).offset().top;
            dd.selectionProxy.css("top", top - 5);

            var insertBefore = Math.max(0, Math.min(Math.round(top / _grid.getOptions().rowHeight), _grid.getDataLength()));
            if (insertBefore !== dd.insertBefore) {
                var eventData = {
                    "rows": dd.selectedRows,
                    "insertBefore": insertBefore
                };

                if (_self.onBeforeMoveRows.notify(eventData) === false) {
                    dd.guide.css("top", -1000);
                    dd.canMove = false;
                } else {
                    dd.guide.css("top", insertBefore * _grid.getOptions().rowHeight);
                    dd.canMove = true;
                }

                dd.insertBefore = insertBefore;
            }
        }

        function handleDragEnd(e, dd) {
            if (!_dragging) {
                return;
            }
            _dragging = false;
            e.stopImmediatePropagation();

            dd.guide.remove();
            dd.selectionProxy.remove();

            if (dd.canMove) {
                var eventData = {
                    "rows": dd.selectedRows,
                    "insertBefore": dd.insertBefore
                };
                // TODO:  _grid.remapCellCssClasses ?
                _self.onMoveRows.notify(eventData);
            }
        }

        $.extend(this, {
            "onBeforeMoveRows": new Slick.Event(),
            "onMoveRows": new Slick.Event(),

            "init": init,
            "destroy": destroy
        });
    }
})(jQuery);

//#endregion

//#region plugins/RowSelectionModel

(function ($) {
    // register namespace
    $.extend(true, window, {
        "Slick": {
            "RowSelectionModel": RowSelectionModel
        }
    });

    function RowSelectionModel(options) {
        var _grid;
        var _ranges = [];
        var _self = this;
        var _handler = new Slick.EventHandler();
        var _inHandler;
        var _options;
        var _defaults = {
            selectActiveRow: true
        };

        function init(grid) {
            _options = $.extend(true, {}, _defaults, options);
            _grid = grid;
            _handler.subscribe(_grid.onActiveCellChanged,
                wrapHandler(handleActiveCellChange));
            _handler.subscribe(_grid.onKeyDown,
                wrapHandler(handleKeyDown));
            _handler.subscribe(_grid.onClick,
                wrapHandler(handleClick));
        }

        function destroy() {
            _handler.unsubscribeAll();
        }

        function wrapHandler(handler) {
            return function () {
                if (!_inHandler) {
                    _inHandler = true;
                    handler.apply(this, arguments);
                    _inHandler = false;
                }
            };
        }

        function rangesToRows(ranges) {
            var rows = [];
            for (var i = 0; i < ranges.length; i++) {
                for (var j = ranges[i].fromRow; j <= ranges[i].toRow; j++) {
                    rows.push(j);
                }
            }
            return rows;
        }

        function rowsToRanges(rows) {
            var ranges = [];
            var lastCell = _grid.getColumns().length - 1;
            for (var i = 0; i < rows.length; i++) {
                ranges.push(new Slick.Range(rows[i], 0, rows[i], lastCell));
            }
            return ranges;
        }

        function getRowsRange(from, to) {
            var i, rows = [];
            for (i = from; i <= to; i++) {
                rows.push(i);
            }
            for (i = to; i < from; i++) {
                rows.push(i);
            }
            return rows;
        }

        function getSelectedRows() {
            return rangesToRows(_ranges);
        }

        function setSelectedRows(rows) {
            setSelectedRanges(rowsToRanges(rows));
        }

        function setSelectedRanges(ranges) {
            // simle check for: empty selection didn't change, prevent firing onSelectedRangesChanged
            if ((!_ranges || _ranges.length === 0) && (!ranges || ranges.length === 0)) { return; }
            _ranges = ranges;
            _self.onSelectedRangesChanged.notify(_ranges);
        }

        function getSelectedRanges() {
            return _ranges;
        }

        function handleActiveCellChange(e, data) {
            if (_options.selectActiveRow && data.row != null) {
                setSelectedRanges([new Slick.Range(data.row, 0, data.row, _grid.getColumns().length - 1)]);
            }
        }

        function handleKeyDown(e) {
            var activeRow = _grid.getActiveCell();
            if (activeRow && e.shiftKey && !e.ctrlKey && !e.altKey && !e.metaKey && (e.which == 38 || e.which == 40)) {
                var selectedRows = getSelectedRows();
                selectedRows.sort(function (x, y) {
                    return x - y
                });

                if (!selectedRows.length) {
                    selectedRows = [activeRow.row];
                }

                var top = selectedRows[0];
                var bottom = selectedRows[selectedRows.length - 1];
                var active;

                if (e.which == 40) {
                    active = activeRow.row < bottom || top == bottom ? ++bottom : ++top;
                } else {
                    active = activeRow.row < bottom ? --bottom : --top;
                }

                if (active >= 0 && active < _grid.getDataLength()) {
                    _grid.scrollRowIntoView(active);
                    _ranges = rowsToRanges(getRowsRange(top, bottom));
                    setSelectedRanges(_ranges);
                }

                e.preventDefault();
                e.stopPropagation();
            }
        }

        function handleClick(e) {
            var cell = _grid.getCellFromEvent(e);
            if (!cell || !_grid.canCellBeActive(cell.row, cell.cell)) {
                return false;
            }

            if (!_grid.getOptions().multiSelect || (
                !e.ctrlKey && !e.shiftKey && !e.metaKey)) {
                return false;
            }

            var selection = rangesToRows(_ranges);
            var idx = $.inArray(cell.row, selection);

            if (idx === -1 && (e.ctrlKey || e.metaKey)) {
                selection.push(cell.row);
                _grid.setActiveCell(cell.row, cell.cell);
            } else if (idx !== -1 && (e.ctrlKey || e.metaKey)) {
                selection = $.grep(selection, function (o, i) {
                    return (o !== cell.row);
                });
                _grid.setActiveCell(cell.row, cell.cell);
            } else if (selection.length && e.shiftKey) {
                var last = selection.pop();
                var from = Math.min(cell.row, last);
                var to = Math.max(cell.row, last);
                selection = [];
                for (var i = from; i <= to; i++) {
                    if (i !== last) {
                        selection.push(i);
                    }
                }
                selection.push(last);
                _grid.setActiveCell(cell.row, cell.cell);
            }

            _ranges = rowsToRanges(selection);
            setSelectedRanges(_ranges);
            e.stopImmediatePropagation();

            return true;
        }

        $.extend(this, {
            "getSelectedRows": getSelectedRows,
            "setSelectedRows": setSelectedRows,

            "getSelectedRanges": getSelectedRanges,
            "setSelectedRanges": setSelectedRanges,

            "init": init,
            "destroy": destroy,

            "onSelectedRangesChanged": new Slick.Event()
        });
    }
})(jQuery);

//#endregion End plugins/RowSelectionModel

//#region 

//#endregion End