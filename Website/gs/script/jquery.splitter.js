//#region Splitter

/*
 * jQuery.splitter.js - two-pane splitter window plugin
 *
 * version 1.6 (2010/01/03)
 * version 1.61 (2012/05/09) -- Fixes by Roger Martin
 *  * Added check in window resize event handler to run only when the target is the window. This fixes a breaking
 *    change introduced in jQuery 1.6.
 *  * Added support for IE 9+
 * version 1.62 (2012/05/16) -- Fixes by Roger Martin
 *  * Included bottom padding of body and html elements when calculating height. This elimates vertical scroll bar and thus a need for overflow:none on the body element
 * version 1.63 (2012/08/12) -- Fixes by Roger Martin
 *  * Changed curCSS to css (curCSS was removed in jQuery 1.8)
 * version 1.64 (2013/01/08) -- Fixes by Roger Martin
 *  * sizeLeft and sizeRight was being ignored when cookie option was used
 * version 1.65 (2013/01/09) -- Fixes by Roger Martin
 *  * Fixed issue where scrollbars were still appearing in IE.
 * version 1.66 (2015/10/16) -- Fixes by Roger Martin
    Removed references to $.browser
 * version 1.67 (2015/10/28) -- Fixes by Roger Martin
    Changed references to $.cookie to Gs.Vars.Cookies (Gs.Vars.Cookies is defined by noConflict() in Gs.Utils.Init. Change required by upgrade from jQuery cookies plugin to JavaScript Cookie)
 * version 1.68 (2016/01/11) -- Fixes by Roger Martin
    Changed z-index of splitter bar from 100 to 2 (required to prevent it from showing through the dialogs from the ribbon toolbar)
 * version 1.69 (2016/10/10) -- Fixes by Roger Martin
    Replaced jQuery bind() and unbind() functions with on() and off().
 *
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */

/**
 * The splitter() plugin implements a two-pane resizable splitter window.
 * The selected elements in the jQuery object are converted to a splitter;
 * each selected element should have two child elements, used for the panes
 * of the splitter. The plugin adds a third child element for the splitbar.
 *
 * For more details see: http://www.methvin.com/splitter/
 *
 *
 * @example $('#MySplitter').splitter();
 * @desc Create a vertical splitter with default settings
 *
 * @example $('#MySplitter').splitter({type: 'h', accessKey: 'M'});
 * @desc Create a horizontal splitter resizable via Alt+Shift+M
 *
 * @name splitter
 * @type jQuery
 * @param Object options Options for the splitter (not required)
 * @cat Plugins/Splitter
 * @return jQuery
 * @author Dave Methvin (dave.methvin@gmail.com)
 */
; (function ($) {

    var splitterCounter = 0;

    $.fn.splitter = function (args) {
        args = args || {};
        return this.each(function () {
            if ($(this).is(".splitter"))	// already a splitter
                return;
            var zombie;		// left-behind splitbar for outline resizes
            function setBarState(state) {
                bar.removeClass(opts.barStateClasses).addClass(state);
            }
            function startSplitMouse(evt) {
                if (evt.which != 1)
                    return;		// left button only
                bar.removeClass(opts.barHoverClass);
                if (opts.outline) {
                    zombie = zombie || bar.clone(false).insertAfter(A);
                    bar.removeClass(opts.barDockedClass);
                }
                setBarState(opts.barActiveClass)
                // Safari selects A/B text on a move; iframes capture mouse events so hide them
                panes.css("-webkit-user-select", "none").find("iframe").addClass(opts.iframeClass);
                A._posSplit = A[0][opts.pxSplit] - evt[opts.eventPos];
                $(document)
             .on("mousemove" + opts.eventNamespace, doSplitMouse)
             .on("mouseup" + opts.eventNamespace, endSplitMouse);
            }
            function doSplitMouse(evt) {
                var pos = A._posSplit + evt[opts.eventPos],
             range = Math.max(0, Math.min(pos, splitter._DA - bar._DA)),
             limit = Math.max(A._min, splitter._DA - B._max,
                     Math.min(pos, A._max, splitter._DA - bar._DA - B._min));
                if (opts.outline) {
                    // Let docking splitbar be dragged to the dock position, even if min width applies
                    if ((opts.dockPane == A && pos < Math.max(A._min, bar._DA)) ||
                    (opts.dockPane == B && pos > Math.min(pos, A._max, splitter._DA - bar._DA - B._min))) {
                        bar.addClass(opts.barDockedClass).css(opts.origin, range);
                    }
                    else {
                        bar.removeClass(opts.barDockedClass).css(opts.origin, limit);
                    }
                    bar._DA = bar[0][opts.pxSplit];
                } else
                    resplit(pos);
                setBarState(pos == limit ? opts.barActiveClass : opts.barLimitClass);
            }
            function endSplitMouse(evt) {
                setBarState(opts.barNormalClass);
                bar.addClass(opts.barHoverClass);
                var pos = A._posSplit + evt[opts.eventPos];
                if (opts.outline) {
                    zombie.remove(); zombie = null;
                    resplit(pos);
                }
                panes.css("-webkit-user-select", "text").find("iframe").removeClass(opts.iframeClass);
                $(document)
             .off("mousemove" + opts.eventNamespace + " mouseup" + opts.eventNamespace);
            }
            function resplit(pos) {
                bar._DA = bar[0][opts.pxSplit];		// bar size may change during dock
                // Constrain new splitbar position to fit pane size and docking limits
                if ((opts.dockPane == A && pos < Math.max(A._min, bar._DA)) ||
                (opts.dockPane == B && pos > Math.min(pos, A._max, splitter._DA - bar._DA - B._min))) {
                    bar.addClass(opts.barDockedClass);
                    bar._DA = bar[0][opts.pxSplit];
                    pos = opts.dockPane == A ? 0 : splitter._DA - bar._DA;
                    if (bar._pos == null)
                        bar._pos = A[0][opts.pxSplit];
                }
                else {
                    bar.removeClass(opts.barDockedClass);
                    bar._DA = bar[0][opts.pxSplit];
                    bar._pos = null;
                    pos = Math.max(A._min, splitter._DA - B._max,
                     Math.min(pos, A._max, splitter._DA - bar._DA - B._min));
                }
                // Resize/position the two panes
                bar.css(opts.origin, pos).css(opts.fixed, splitter._DF);
                A.css(opts.origin, 0).css(opts.split, pos).css(opts.fixed, splitter._DF);
                B.css(opts.origin, pos + bar._DA)
             .css(opts.split, splitter._DA - bar._DA - pos).css(opts.fixed, splitter._DF);
                for (i = 0; i <= splitterCounter; i++) {
                    panes.trigger("resize" + eventNamespaceBase + i);
                }
            }
            function dimSum(jq, dims) {
                // Opera returns -1 for missing min/max width, turn into 0
                var sum = 0;
                for (var i = 1; i < arguments.length; i++)
                    sum += Math.max(parseInt(jq.css(arguments[i]), 10) || 0, 0);
                return sum;
            }

            // Determine settings based on incoming opts, element classes, and defaults
            var vh = (args.splitHorizontal ? 'h' : args.splitVertical ? 'v' : args.type) || 'v';
            var eventNamespaceBase = ".splitter";
            var opts = $.extend({
                // Defaults here allow easy use with ThemeRoller
                splitterClass: "splitter gsp-ui-widget gsp-ui-widget-content",
                paneClass: "splitter-pane",
                barClass: "splitter-bar",
                barNormalClass: "gsp-ui-state-default",			// splitbar normal
                barHoverClass: "gsp-ui-state-hover",			// splitbar mouse hover
                barActiveClass: "gsp-ui-state-highlight",		// splitbar being moved
                barLimitClass: "gsp-ui-state-error",			// splitbar at limit
                iframeClass: "splitter-iframe-hide",		// hide iframes during split
                eventNamespace: eventNamespaceBase + (++splitterCounter),
                pxPerKey: 8,			// splitter px moved per keypress
                tabIndex: 0,			// tab order indicator
                accessKey: ''			// accessKey for splitbar
            }, {
                // user can override
                v: {					// Vertical splitters:
                    keyLeft: 39, keyRight: 37, cursor: "e-resize",
                    barStateClass: "splitter-bar-vertical",
                    barDockedClass: "splitter-bar-vertical-docked"
                },
                h: {					// Horizontal splitters:
                    keyTop: 40, keyBottom: 38, cursor: "n-resize",
                    barStateClass: "splitter-bar-horizontal",
                    barDockedClass: "splitter-bar-horizontal-docked"
                }
            }[vh], args, {
                // user cannot override
                v: {					// Vertical splitters:
                    type: 'v', eventPos: "pageX", origin: "left",
                    split: "width", pxSplit: "offsetWidth", side1: "Left", side2: "Right",
                    fixed: "height", pxFixed: "offsetHeight", side3: "Top", side4: "Bottom"
                },
                h: {					// Horizontal splitters:
                    type: 'h', eventPos: "pageY", origin: "top",
                    split: "height", pxSplit: "offsetHeight", side1: "Top", side2: "Bottom",
                    fixed: "width", pxFixed: "offsetWidth", side3: "Left", side4: "Right"
                }
            }[vh]);
            opts.barStateClasses = [opts.barNormalClass, opts.barHoverClass, opts.barActiveClass, opts.barLimitClass].join(' ');

            // Create jQuery object closures for splitter and both panes
            var splitter = $(this).css({ position: "relative" }).addClass(opts.splitterClass);
            var panes = $(">*", splitter[0]).addClass(opts.paneClass).css({
                position: "absolute", 			// positioned inside splitter container
                "z-index": "1",					// splitbar is positioned above
                "-moz-outline-style": "none"	// don't show dotted outline
            });
            var A = $(panes[0]), B = $(panes[1]);	// A = left/top, B = right/bottom
            opts.dockPane = opts.dock && (/right|bottom/.test(opts.dock) ? B : A);

            // Focuser element, provides keyboard support; title is shown by Opera accessKeys
            var focuser = $('<a href="javascript:void(0)"></a>')
         .attr({ accessKey: opts.accessKey, tabIndex: opts.tabIndex, title: opts.splitbarClass })
         .on("focus" + opts.eventNamespace,
             function () { this.focus(); bar.addClass(opts.barActiveClass) })
         .on("keydown" + opts.eventNamespace, function (e) {
             var key = e.which || e.keyCode;
             var dir = key == opts["key" + opts.side1] ? 1 : key == opts["key" + opts.side2] ? -1 : 0;
             if (dir)
                 resplit(A[0][opts.pxSplit] + dir * opts.pxPerKey, false);
         })
         .on("blur" + opts.eventNamespace,
             function () { bar.removeClass(opts.barActiveClass) });

            // Splitbar element
            var bar = $('<div></div>')
         .insertAfter(A).addClass(opts.barClass).addClass(opts.barStateClass)
         .append(focuser).attr({ unselectable: "on" })
         .css({
             position: "absolute", "user-select": "none", "-webkit-user-select": "none",
             "-khtml-user-select": "none", "-moz-user-select": "none", "z-index": "2"  //[RDM] Changed z-index from 100 to 2
         })
         .on("mousedown" + opts.eventNamespace, startSplitMouse)
         .on("mouseover" + opts.eventNamespace, function () {
             $(this).addClass(opts.barHoverClass);
         })
         .on("mouseout" + opts.eventNamespace, function () {
             $(this).removeClass(opts.barHoverClass);
         });
            // Use our cursor unless the style specifies a non-default cursor
            if (/^(auto|default|)$/.test(bar.css("cursor")))
                bar.css("cursor", opts.cursor);

            // Cache several dimensions for speed, rather than re-querying constantly
            // These are saved on the A/B/bar/splitter jQuery vars, which are themselves cached
            // DA=dimension adjustable direction, PBF=padding/border fixed, PBA=padding/border adjustable
            bar._DA = bar[0][opts.pxSplit];
            splitter._PBF = dimSum(splitter, "border" + opts.side3 + "Width", "border" + opts.side4 + "Width");
            splitter._PBA = dimSum(splitter, "border" + opts.side1 + "Width", "border" + opts.side2 + "Width");
            A._pane = opts.side1;
            B._pane = opts.side2;
            $.each([A, B], function () {
                this._splitter_style = this.style;
                this._min = opts["min" + this._pane] || dimSum(this, "min-" + opts.split);
                this._max = opts["max" + this._pane] || dimSum(this, "max-" + opts.split) || 9999;
                this._init = opts["size" + this._pane] === true ?
             parseInt($.css(this[0], opts.split), 10) : opts["size" + this._pane]; //[RDM] Changed curCSS to css (curCSS was removed in jQuery 1.8)
            });

            // Determine initial position, get from cookie if specified
            var initPos = A._init;
            if (!isNaN(B._init))	// recalc initial B size as an offset from the top or left side
                initPos = splitter[0][opts.pxSplit] - splitter._PBA - B._init - bar._DA;
            if (opts.cookie) {
                if (!Gs.Vars.Cookies)
                    alert('jQuery.splitter(): jQuery cookie plugin required');
                var cookieVal = parseInt(Gs.Vars.Cookies.get(opts.cookie), 10);
                if (!isNaN(cookieVal))
                    initPos = cookieVal; //[RDM] Overwrite initPos only when we found a cookie (instead of always)
                $(window).on("unload" + opts.eventNamespace, function () {
                    var state = String(bar.css(opts.origin));	// current location of splitbar
                    Gs.Vars.Cookies.set(opts.cookie, state, {
                        expires: opts.cookieExpires || 365,
                        path: opts.cookiePath || document.location.pathname
                    });
                });
            }
            if (isNaN(initPos))	// King Solomon's algorithm
                initPos = Math.round((splitter[0][opts.pxSplit] - splitter._PBA - bar._DA) / 2);

            // Resize event propagation and splitter sizing
            if (opts.anchorToWindow)
                opts.resizeTo = window;
            if (opts.resizeTo) {
                splitter._hadjust = dimSum(splitter, "borderTopWidth", "borderBottomWidth", "marginBottom", "paddingBottom");
                splitter._hadjust += dimSum($('body'), 'paddingBottom'); // Added by Roger
                splitter._hadjust += dimSum($('html'), 'paddingBottom'); // Added by Roger
                splitter._hadjust += 1; // [RDM] Need a fudge factor of one extra pixel to prevent scrollbars in IE & Chrome
                splitter._hmin = Math.max(dimSum(splitter, "minHeight"), 20);
                $(window).on("resize" + opts.eventNamespace, function (e) {
                    if (e.target == window) {
                        var top = splitter.offset().top;
                        var eh = $(opts.resizeTo).height();
                        splitter.css("height", Math.max(eh - top - splitter._hadjust - 0, splitter._hmin) + "px");
                        splitter.trigger("resize" + opts.eventNamespace);
                    }
                }).trigger("resize" + opts.eventNamespace);
            }
            else if (opts.resizeToWidth) {
                $(window).on("resize" + opts.eventNamespace, function (e) {
                    if (e.target == window) {
                        splitter.trigger("resize" + opts.eventNamespace);
                    }
                });
            }

            // Docking support
            if (opts.dock) {
                splitter
             .on("toggleDock" + opts.eventNamespace, function () {
                 var pw = opts.dockPane[0][opts.pxSplit];
                 splitter.trigger(pw ? "dock" + opts.eventNamespace : "undock" + opts.eventNamespace);
             })
             .on("dock" + opts.eventNamespace, function () {
                 var pw = A[0][opts.pxSplit];
                 if (!pw) return;
                 bar._pos = pw;
                 var x = {};
                 x[opts.origin] = opts.dockPane == A ? 0 :
                     splitter[0][opts.pxSplit] - splitter._PBA - bar[0][opts.pxSplit];
                 bar.animate(x, opts.dockSpeed || 1, opts.dockEasing, function () {
                     bar.addClass(opts.barDockedClass);
                     resplit(x[opts.origin]);
                 });
             })
             .on("undock" + opts.eventNamespace, function () {
                 var pw = opts.dockPane[0][opts.pxSplit];
                 if (pw) return;
                 var x = {}; x[opts.origin] = bar._pos + "px";
                 bar.removeClass(opts.barDockedClass)
                     .animate(x, opts.undockSpeed || opts.dockSpeed || 1, opts.undockEasing || opts.dockEasing, function () {
                         resplit(bar._pos);
                         bar._pos = null;
                     });
             });
                if (opts.dockKey)
                    $('<a title="' + opts.splitbarClass + ' toggle dock" href="javascript:void(0)"></a>')
                 .attr({ accessKey: opts.dockKey, tabIndex: -1 }).appendTo(bar)
                 .on("focus", function () {
                     splitter.trigger("toggleDock" + opts.eventNamespace); this.blur();
                 });
                bar.on("dblclick", function () { splitter.trigger("toggleDock" + opts.eventNamespace); });
            }


            // Resize event handler; triggered immediately to set initial position
            splitter
         .on("destroy" + opts.eventNamespace, function () {
             $([window, document]).off(opts.eventNamespace);
             bar.off().remove();
             panes.removeClass(opts.paneClass);
             splitter
                 .removeClass(opts.splitterClass)
                 .add(panes)
                     .off(opts.eventNamespace)
                     .attr("style", function (el) {
                         return this._splitter_style || "";	//TODO: save style
                     });
             splitter = bar = focuser = panes = A = B = opts = args = null;
         })
         .on("resize" + opts.eventNamespace, function (e, size) {
             // Custom events bubble in jQuery 1.3; avoid recursion
             if (e.target != this) return;
             // Determine new width/height of splitter container
             splitter._DF = splitter[0][opts.pxFixed] - splitter._PBF;
             splitter._DA = splitter[0][opts.pxSplit] - splitter._PBA;
             // Bail if splitter isn't visible or content isn't there yet
             if (splitter._DF <= 0 || splitter._DA <= 0) return;
             // Re-divvy the adjustable dimension; maintain size of the preferred pane
             resplit(!isNaN(size) ? size : (!(opts.sizeRight || opts.sizeBottom) ? A[0][opts.pxSplit] :
                 splitter._DA - B[0][opts.pxSplit] - bar._DA));
             setBarState(opts.barNormalClass);
         })
         .trigger("resize" + opts.eventNamespace, [initPos]);
        });
    };

})(jQuery);

//#endregion End Splitter
