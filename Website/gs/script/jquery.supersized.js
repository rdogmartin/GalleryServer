//#region supersized

/*
	supersized.3.2.7.js
	Supersized - Fullscreen Slideshow jQuery Plugin
	Version : 3.2.7
	Site	: www.buildinternet.com/project/supersized
	
	Author	: Sam Dunn
	Company : One Mighty Roar (www.onemightyroar.com)
	License : MIT License / GPL License

*/

(function ($) {

    $.supersized = function (options) {

        // If caller requested the api variable, return it. It is unfortunately a global variable, so this isn't really necessary, but TypeScript
        // doesn't like it when you invoke variables that aren't properly defined, so we added this function.
        if (options === 'getApi') {
            return api;
        }

        /* Variables
	----------------------------*/
        var base = this;

        base.init = function () {
            // Combine options and vars
            $.supersized.vars = $.extend($.supersized.vars, $.supersized.themeVars);
            $.supersized.vars.options = $.extend({}, $.supersized.defaultOptions, $.supersized.themeOptions, options);
            base.options = $.supersized.vars.options;

            base._build();
        };


        /* Build Elements
----------------------------*/
        base._build = function () {
            // Add in slide markers
            var thisSlide = 0,
				slideSet = '',
		markers = '',
		markerContent,
		thumbMarkers = '',
		thumbImage;


            // Hide current page contents and add Supersized Elements
            $('body').children(':visible').hide().addClass('supersized_hidden');
            $('body').append($($.supersized.vars.options.html_template), '<div id="supersized-loader"></div><ul id="supersized"></ul>');

            var el = '#supersized';
            // Access to jQuery and DOM versions of element
            base.$el = $(el);
            base.el = el;
            vars = $.supersized.vars;
            vars.$container = base.$el;
            // Add a reverse reference to the DOM object
            base.$el.data("supersized", base);
            api = base.$el.data('supersized');


            while (thisSlide <= base.options.slides.length - 1) {
                //Determine slide link content
                switch (base.options.slide_links) {
                    case 'num':
                        markerContent = thisSlide;
                        break;
                    case 'name':
                        markerContent = base.options.slides[thisSlide].title;
                        break;
                    case 'blank':
                        markerContent = '';
                        break;
                }

                slideSet = slideSet + '<li class="slide-' + thisSlide + '"></li>';

                if (thisSlide == base.options.start_slide - 1) {
                    // Slide links
                    if (base.options.slide_links) markers = markers + '<li class="slide-link-' + thisSlide + ' current-slide"><a>' + markerContent + '</a></li>';
                    // Slide Thumbnail Links
                    if (base.options.thumb_links) {
                        base.options.slides[thisSlide].thumb ? thumbImage = base.options.slides[thisSlide].thumb : thumbImage = base.options.slides[thisSlide].image;
                        thumbMarkers = thumbMarkers + '<li class="thumb' + thisSlide + ' current-thumb"><img src="' + thumbImage + '"/></li>';
                    };
                } else {
                    // Slide links
                    if (base.options.slide_links) markers = markers + '<li class="slide-link-' + thisSlide + '" ><a>' + markerContent + '</a></li>';
                    // Slide Thumbnail Links
                    if (base.options.thumb_links) {
                        base.options.slides[thisSlide].thumb ? thumbImage = base.options.slides[thisSlide].thumb : thumbImage = base.options.slides[thisSlide].image;
                        thumbMarkers = thumbMarkers + '<li class="thumb' + thisSlide + '"><img src="' + thumbImage + '"/></li>';
                    };
                }
                thisSlide++;
            }

            if (base.options.slide_links) $(vars.slide_list).html(markers);
            if (base.options.thumb_links && vars.thumb_tray.length) {
                $(vars.thumb_tray).append('<ul id="' + vars.thumb_list.replace('#', '') + '">' + thumbMarkers + '</ul>');
            }

            $(base.el).append(slideSet);

            // Add in thumbnails
            if (base.options.thumbnail_navigation) {
                // Load previous thumbnail
                vars.current_slide - 1 < 0 ? prevThumb = base.options.slides.length - 1 : prevThumb = vars.current_slide - 1;
                $(vars.prev_thumb).show().html($("<img/>").attr("src", base.options.slides[prevThumb].image));

                // Load next thumbnail
                vars.current_slide == base.options.slides.length - 1 ? nextThumb = 0 : nextThumb = vars.current_slide + 1;
                $(vars.next_thumb).show().html($("<img/>").attr("src", base.options.slides[nextThumb].image));
            }

            base._start(); // Get things started
        };


        /* Initialize
----------------------------*/
        base._start = function () {

            // Determine if starting slide random
            if (base.options.start_slide) {
                vars.current_slide = base.options.start_slide - 1;
            } else {
                vars.current_slide = Math.floor(Math.random() * base.options.slides.length);	// Generate random slide number
            }

            // If links should open in new window
            var linkTarget = base.options.new_window ? ' target="_blank"' : '';

            // Set slideshow quality (Supported only in FF and IE, no Webkit)
            if (base.options.performance == 3) {
                base.$el.addClass('speed'); 		// Faster transitions
            } else if ((base.options.performance == 1) || (base.options.performance == 2)) {
                base.$el.addClass('quality');	// Higher image quality
            }

            // Shuffle slide order if needed		
            if (base.options.random) {
                arr = base.options.slides;
                for (var j, x, i = arr.length; i; j = parseInt(Math.random() * i), x = arr[--i], arr[i] = arr[j], arr[j] = x);	// Fisher-Yates shuffle algorithm (jsfromhell.com/array/shuffle)
                base.options.slides = arr;
            }

            /*-----Load initial set of images-----*/

            if (base.options.slides.length > 1) {
                if (base.options.slides.length > 2) {
                    // Set previous image
                    vars.current_slide - 1 < 0 ? loadPrev = base.options.slides.length - 1 : loadPrev = vars.current_slide - 1;	// If slide is 1, load last slide as previous
                    var imageLink = (base.options.slides[loadPrev].url) ? "href='" + base.options.slides[loadPrev].url + "'" : "";

                    var imgPrev = $('<img src="' + base.options.slides[loadPrev].image + '"/>');
                    var slidePrev = base.el + ' li:eq(' + loadPrev + ')';
                    imgPrev.appendTo(slidePrev).wrap('<a ' + imageLink + linkTarget + '></a>').parent().parent().addClass('image-loading prevslide');

                    imgPrev.load(function () {
                        $(this).data('origWidth', $(this).width()).data('origHeight', $(this).height());
                        base.resizeNow();	// Resize background image
                    });	// End Load
                }
            } else {
                // Slideshow turned off if there is only one slide
                //base.options.slideshow = 0; //[RDM] Commented out because this disables buttons when there is only one slide
            }

            // Set current image
            imageLink = (api.getField('url')) ? "href='" + api.getField('url') + "'" : "";
            var img = $('<img src="' + api.getField('image') + '"/>');

            var slideCurrent = base.el + ' li:eq(' + vars.current_slide + ')';
            img.appendTo(slideCurrent).wrap('<a ' + imageLink + linkTarget + '></a>').parent().parent().addClass('image-loading activeslide').css('visibility', 'visible');

            img.load(function () {
                base._origDim($(this));
                base.resizeNow();	// Resize background image
                base.launch();
                if (typeof theme != 'undefined' && typeof theme._init == "function") theme._init();	// Load Theme
            });

            if (base.options.slides.length > 1) {
                // Set next image
                vars.current_slide == base.options.slides.length - 1 ? loadNext = 0 : loadNext = vars.current_slide + 1;	// If slide is last, load first slide as next
                imageLink = (base.options.slides[loadNext].url) ? "href='" + base.options.slides[loadNext].url + "'" : "";

                var imgNext = $('<img src="' + base.options.slides[loadNext].image + '"/>');
                var slideNext = base.el + ' li:eq(' + loadNext + ')';
                imgNext.appendTo(slideNext).wrap('<a ' + imageLink + linkTarget + '></a>').parent().parent().addClass('image-loading');

                imgNext.load(function () {
                    $(this).data('origWidth', $(this).width()).data('origHeight', $(this).height());
                    base.resizeNow();	// Resize background image
                });	// End Load
            }
            /*-----End load initial images-----*/

            //  Hide elements to be faded in
            base.$el.css('visibility', 'hidden');
            $('.load-item').hide();

        };


        /* Launch Supersized
		----------------------------*/
        base.launch = function () {

            //base.$el.css('visibility', 'visible');
            $('#supersized-loader').remove();		//Hide loading animation

            // Call theme function for before slide transition
            if (typeof theme != 'undefined' && typeof theme.beforeAnimation == "function") theme.beforeAnimation('next');
            $('.load-item').show();

            // Keyboard Navigation
            if (base.options.keyboard_nav) {
                $(document.documentElement).on('keyup.supersized', function (event) {

                    if (vars.in_animation) return false;		// Abort if currently animating
                    if ($(document.activeElement).is("input, textarea")) return false; // Abort if active element is an input or a textarea.

                    // Left Arrow or Down Arrow
                    if ((event.keyCode == 37) || (event.keyCode == 40)) {
                        clearInterval(vars.slideshow_interval);	// Stop slideshow, prevent buildup
                        base.prevSlide();

                        // Right Arrow or Up Arrow
                    } else if ((event.keyCode == 39) || (event.keyCode == 38)) {
                        clearInterval(vars.slideshow_interval);	// Stop slideshow, prevent buildup
                        base.nextSlide();

                        // Spacebar	
                    } else if (event.keyCode == 32 && !vars.hover_pause) {
                        clearInterval(vars.slideshow_interval);	// Stop slideshow, prevent buildup
                        base.playToggle();
                    }

                });
            }

            // Pause when hover on image
            if (base.options.slideshow && base.options.pause_hover) {
                $(base.el).hover(function () {
                    if (vars.in_animation) return false;		// Abort if currently animating
                    vars.hover_pause = true;	// Mark slideshow paused from hover
                    if (!vars.is_paused) {
                        vars.hover_pause = 'resume';	// It needs to resume afterwards
                        base.playToggle();
                    }
                }, function () {
                    if (vars.hover_pause == 'resume') {
                        base.playToggle();
                        vars.hover_pause = false;
                    }
                });
            }

            if (base.options.slide_links) {
                // Slide marker clicked
                $(vars.slide_list + '> li').click(function () {

                    index = $(vars.slide_list + '> li').index(this);
                    targetSlide = index + 1;

                    base.goTo(targetSlide);
                    return false;

                });
            }

            // Thumb marker clicked
            if (base.options.thumb_links) {
                $(vars.thumb_list + '> li').click(function () {

                    index = $(vars.thumb_list + '> li').index(this);
                    targetSlide = index + 1;

                    api.goTo(targetSlide);
                    return false;

                });
            }

            // Start slideshow if enabled
            if (base.options.slideshow && base.options.slides.length > 1) {

                // Start slideshow if autoplay enabled
                if (base.options.autoplay && base.options.slides.length > 1) {
                    vars.slideshow_interval = setInterval(base.nextSlide, base.options.slide_interval);	// Initiate slide interval
                } else {
                    vars.is_paused = true;	// Mark as paused
                }

                //Prevent navigation items from being dragged					
                $('.load-item img').bind("contextmenu mousedown", function () {
                    return false;
                });

            }

            // Adjust image when browser is resized
            $(window).resize(function () {
                base.resizeNow();
            });

        };


        /* Resize Images
----------------------------*/
        base.resizeNow = function () {

            return base.$el.each(function () {
                //  Resize each image seperately
                $('img', base.el).each(function () {

                    thisSlide = $(this);
                    var ratio = (thisSlide.data('origHeight') / thisSlide.data('origWidth')).toFixed(2);	// Define image ratio

                    // Gather browser size
                    var browserwidth = base.$el.width(),
						browserheight = base.$el.height(),
						offset;

                    /*-----Resize Image-----*/
                    if (base.options.fit_always) {	// Fit always is enabled
                        if ((browserheight / browserwidth) > ratio) {
                            resizeWidth();
                        } else {
                            resizeHeight();
                        }
                    } else {	// Normal Resize
                        if ((browserheight <= base.options.min_height) && (browserwidth <= base.options.min_width)) {	// If window smaller than minimum width and height

                            if ((browserheight / browserwidth) > ratio) {
                                base.options.fit_landscape && ratio < 1 ? resizeWidth(true) : resizeHeight(true);	// If landscapes are set to fit
                            } else {
                                base.options.fit_portrait && ratio >= 1 ? resizeHeight(true) : resizeWidth(true);		// If portraits are set to fit
                            }

                        } else if (browserwidth <= base.options.min_width) {		// If window only smaller than minimum width

                            if ((browserheight / browserwidth) > ratio) {
                                base.options.fit_landscape && ratio < 1 ? resizeWidth(true) : resizeHeight();	// If landscapes are set to fit
                            } else {
                                base.options.fit_portrait && ratio >= 1 ? resizeHeight() : resizeWidth(true);		// If portraits are set to fit
                            }

                        } else if (browserheight <= base.options.min_height) {	// If window only smaller than minimum height

                            if ((browserheight / browserwidth) > ratio) {
                                base.options.fit_landscape && ratio < 1 ? resizeWidth() : resizeHeight(true);	// If landscapes are set to fit
                            } else {
                                base.options.fit_portrait && ratio >= 1 ? resizeHeight(true) : resizeWidth();		// If portraits are set to fit
                            }

                        } else {	// If larger than minimums

                            if ((browserheight / browserwidth) > ratio) {
                                base.options.fit_landscape && ratio < 1 ? resizeWidth() : resizeHeight();	// If landscapes are set to fit
                            } else {
                                base.options.fit_portrait && ratio >= 1 ? resizeHeight() : resizeWidth();		// If portraits are set to fit
                            }

                        }
                    }
                    /*-----End Image Resize-----*/


                    /*-----Resize Functions-----*/

                    function resizeWidth(minimum) {
                        if (minimum) {	// If minimum height needs to be considered
                            if (thisSlide.width() < browserwidth || thisSlide.width() < base.options.min_width) {
                                if (thisSlide.width() * ratio >= base.options.min_height) {
                                    thisSlide.width(base.options.min_width);
                                    thisSlide.height(thisSlide.width() * ratio);
                                } else {
                                    resizeHeight();
                                }
                            }
                        } else {
                            if (base.options.min_height >= browserheight && !base.options.fit_landscape) {	// If minimum height needs to be considered
                                if (browserwidth * ratio >= base.options.min_height || (browserwidth * ratio >= base.options.min_height && ratio <= 1)) {	// If resizing would push below minimum height or image is a landscape
                                    thisSlide.width(browserwidth);
                                    thisSlide.height(browserwidth * ratio);
                                } else if (ratio > 1) {		// Else the image is portrait
                                    thisSlide.height(base.options.min_height);
                                    thisSlide.width(thisSlide.height() / ratio);
                                } else if (thisSlide.width() < browserwidth) {
                                    thisSlide.width(browserwidth);
                                    thisSlide.height(thisSlide.width() * ratio);
                                }
                            } else {	// Otherwise, resize as normal
                                thisSlide.width(browserwidth);
                                thisSlide.height(browserwidth * ratio);
                            }
                        }
                    };

                    function resizeHeight(minimum) {
                        if (minimum) {	// If minimum height needs to be considered
                            if (thisSlide.height() < browserheight) {
                                if (thisSlide.height() / ratio >= base.options.min_width) {
                                    thisSlide.height(base.options.min_height);
                                    thisSlide.width(thisSlide.height() / ratio);
                                } else {
                                    resizeWidth(true);
                                }
                            }
                        } else {	// Otherwise, resized as normal
                            if (base.options.min_width >= browserwidth) {	// If minimum width needs to be considered
                                if (browserheight / ratio >= base.options.min_width || ratio > 1) {	// If resizing would push below minimum width or image is a portrait
                                    thisSlide.height(browserheight);
                                    thisSlide.width(browserheight / ratio);
                                } else if (ratio <= 1) {		// Else the image is landscape
                                    thisSlide.width(base.options.min_width);
                                    thisSlide.height(thisSlide.width() * ratio);
                                }
                            } else {	// Otherwise, resize as normal
                                thisSlide.height(browserheight);
                                thisSlide.width(browserheight / ratio);
                            }
                        }
                    };

                    /*-----End Resize Functions-----*/

                    if (thisSlide.parents('li').hasClass('image-loading')) {
                        $('.image-loading').removeClass('image-loading');
                    }

                    // Horizontally Center
                    if (base.options.horizontal_center) {
                        $(this).css('left', (browserwidth - $(this).width()) / 2);
                    }

                    // Vertically Center
                    if (base.options.vertical_center) {
                        $(this).css('top', (browserheight - $(this).height()) / 2);
                    }

                });

                // Basic image drag and right click protection
                if (base.options.image_protect) {

                    $('img', base.el).bind("contextmenu mousedown", function () {
                        return false;
                    });

                }

                return false;

            });

        };


        /* Next Slide
----------------------------*/
        base.nextSlide = function () {
            if (base.options.slideshow && !vars.is_paused && base.options.auto_exit && (vars.current_slide == base.options.slides.length - 1)) {
                // We're on the last slide of a running slideshow where auto_exit is enabled, so exit.
                base.destroy(true);
                return false;
            }

            var old_slide_number = vars.current_slide;
            // Get the slide number of new slide
            if (vars.current_slide < base.options.slides.length - 1) {
                vars.current_slide++;
            } else if (base.options.loop) {
                vars.current_slide = 0;
            }

            if (old_slide_number == vars.current_slide) {
                vars.in_animation = false;
                return false;
            }

            if (vars.in_animation || !api.options.slideshow) return false;		// Abort if currently animating
            else vars.in_animation = true;		// Otherwise set animation marker

            clearInterval(vars.slideshow_interval);	// Stop slideshow

            var slides = base.options.slides,					// Pull in slides array
			liveslide = base.$el.find('.activeslide');		// Find active slide
            $('.prevslide').removeClass('prevslide');
            liveslide.removeClass('activeslide').addClass('prevslide');	// Remove active class & update previous slide


            var nextslide = $(base.el + ' li:eq(' + vars.current_slide + ')'),
				prevslide = base.$el.find('.prevslide');

            // If hybrid mode is on drop quality for transition
            if (base.options.performance == 1) base.$el.removeClass('quality').addClass('speed');


            /*-----Load Image-----*/

            loadSlide = false;

            vars.current_slide == base.options.slides.length - 1 ? loadSlide = 0 : loadSlide = vars.current_slide + 1;	// Determine next slide

            var targetList = base.el + ' li:eq(' + loadSlide + ')';
            if (!$(targetList).html()) {

                // If links should open in new window
                var linkTarget = base.options.new_window ? ' target="_blank"' : '';

                imageLink = (base.options.slides[loadSlide].url) ? "href='" + base.options.slides[loadSlide].url + "'" : "";	// If link exists, build it
                var img = $('<img src="' + base.options.slides[loadSlide].image + '"/>');

                img.appendTo(targetList).wrap('<a ' + imageLink + linkTarget + '></a>').parent().parent().addClass('image-loading').css('visibility', 'hidden');

                img.load(function () {
                    base._origDim($(this));
                    base.resizeNow();
                });	// End Load
            };

            // Update thumbnails (if enabled)
            if (base.options.thumbnail_navigation == 1) {

                // Load previous thumbnail
                vars.current_slide - 1 < 0 ? prevThumb = base.options.slides.length - 1 : prevThumb = vars.current_slide - 1;
                $(vars.prev_thumb).html($("<img/>").attr("src", base.options.slides[prevThumb].image));

                // Load next thumbnail
                nextThumb = loadSlide;
                $(vars.next_thumb).html($("<img/>").attr("src", base.options.slides[nextThumb].image));

            }



            /*-----End Load Image-----*/


            // Call theme function for before slide transition
            if (typeof theme != 'undefined' && typeof theme.beforeAnimation == "function") theme.beforeAnimation('next');

            //Update slide markers
            if (base.options.slide_links) {
                $('.current-slide').removeClass('current-slide');
                $(vars.slide_list + '> li').eq(vars.current_slide).addClass('current-slide');
            }

            nextslide.css('visibility', 'hidden').addClass('activeslide');	// Update active slide

            switch (base.options.transition) {
                case 0: case 'none':	// No transition
                    nextslide.css('visibility', 'visible'); vars.in_animation = false; base.afterAnimation();
                    break;
                case 1: case 'fade':	// Fade
                    nextslide.css({ opacity: 0, 'visibility': 'visible' }).animate({ opacity: 1, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 2: case 'slideTop':	// Slide Top
                    nextslide.css({ top: -base.$el.height(), 'visibility': 'visible' }).animate({ top: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 3: case 'slideRight':	// Slide Right
                    nextslide.css({ left: base.$el.width(), 'visibility': 'visible' }).animate({ left: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 4: case 'slideBottom': // Slide Bottom
                    nextslide.css({ top: base.$el.height(), 'visibility': 'visible' }).animate({ top: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 5: case 'slideLeft':  // Slide Left
                    nextslide.css({ left: -base.$el.width(), 'visibility': 'visible' }).animate({ left: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 6: case 'carouselRight':	// Carousel Right
                    nextslide.css({ left: base.$el.width(), 'visibility': 'visible' }).animate({ left: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    liveslide.animate({ left: -base.$el.width(), avoidTransforms: false }, base.options.transition_speed);
                    break;
                case 7: case 'carouselLeft':   // Carousel Left
                    nextslide.css({ left: -base.$el.width(), 'visibility': 'visible' }).animate({ left: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    liveslide.animate({ left: base.$el.width(), avoidTransforms: false }, base.options.transition_speed);
                    break;
            }
            return false;
        };


        /* Previous Slide
		----------------------------*/
        base.prevSlide = function () {

            if (vars.in_animation || !api.options.slideshow) return false;		// Abort if currently animating
            else vars.in_animation = true;		// Otherwise set animation marker

            var old_slide_number = vars.current_slide;
            // Get current slide number
            if (vars.current_slide > 0) {
                vars.current_slide--;
            } else if (base.options.loop) {
                vars.current_slide = base.options.slides.length - 1;
            }

            if (old_slide_number == vars.current_slide) {
                vars.in_animation = false;
                return false;
            }

            clearInterval(vars.slideshow_interval);	// Stop slideshow

            var slides = base.options.slides,					// Pull in slides array
				liveslide = base.$el.find('.activeslide');		// Find active slide
            $('.prevslide').removeClass('prevslide');
            liveslide.removeClass('activeslide').addClass('prevslide');		// Remove active class & update previous slide

            var nextslide = $(base.el + ' li:eq(' + vars.current_slide + ')'),
				prevslide = base.$el.find('.prevslide');

            // If hybrid mode is on drop quality for transition
            if (base.options.performance == 1) base.$el.removeClass('quality').addClass('speed');


            /*-----Load Image-----*/

            loadSlide = vars.current_slide;

            var targetList = base.el + ' li:eq(' + loadSlide + ')';
            if (!$(targetList).html()) {
                // If links should open in new window
                var linkTarget = base.options.new_window ? ' target="_blank"' : '';
                imageLink = (base.options.slides[loadSlide].url) ? "href='" + base.options.slides[loadSlide].url + "'" : "";	// If link exists, build it
                var img = $('<img src="' + base.options.slides[loadSlide].image + '"/>');

                img.appendTo(targetList).wrap('<a ' + imageLink + linkTarget + '></a>').parent().parent().addClass('image-loading').css('visibility', 'hidden');

                img.load(function () {
                    base._origDim($(this));
                    base.resizeNow();
                });	// End Load
            };

            // Update thumbnails (if enabled)
            if (base.options.thumbnail_navigation == 1) {

                // Load previous thumbnail
                //prevThumb = loadSlide;
                loadSlide == 0 ? prevThumb = base.options.slides.length - 1 : prevThumb = loadSlide - 1;
                $(vars.prev_thumb).html($("<img/>").attr("src", base.options.slides[prevThumb].image));

                // Load next thumbnail
                vars.current_slide == base.options.slides.length - 1 ? nextThumb = 0 : nextThumb = vars.current_slide + 1;
                $(vars.next_thumb).html($("<img/>").attr("src", base.options.slides[nextThumb].image));
            }

            /*-----End Load Image-----*/


            // Call theme function for before slide transition
            if (typeof theme != 'undefined' && typeof theme.beforeAnimation == "function") theme.beforeAnimation('prev');

            //Update slide markers
            if (base.options.slide_links) {
                $('.current-slide').removeClass('current-slide');
                $(vars.slide_list + '> li').eq(vars.current_slide).addClass('current-slide');
            }

            nextslide.css('visibility', 'hidden').addClass('activeslide');	// Update active slide

            switch (base.options.transition) {
                case 0: case 'none':	// No transition
                    nextslide.css('visibility', 'visible'); vars.in_animation = false; base.afterAnimation();
                    break;
                case 1: case 'fade':	// Fade
                    nextslide.css({ opacity: 0, 'visibility': 'visible' }).animate({ opacity: 1, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 2: case 'slideTop':	// Slide Top (reverse)
                    nextslide.css({ top: base.$el.height(), 'visibility': 'visible' }).animate({ top: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 3: case 'slideRight':	// Slide Right (reverse)
                    nextslide.css({ left: -base.$el.width(), 'visibility': 'visible' }).animate({ left: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 4: case 'slideBottom': // Slide Bottom (reverse)
                    nextslide.css({ top: -base.$el.height(), 'visibility': 'visible' }).animate({ top: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 5: case 'slideLeft':  // Slide Left (reverse)
                    nextslide.css({ left: base.$el.width(), 'visibility': 'visible' }).animate({ left: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    break;
                case 6: case 'carouselRight':	// Carousel Right (reverse)
                    nextslide.css({ left: -base.$el.width(), 'visibility': 'visible' }).animate({ left: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    liveslide.css({ left: 0 }).animate({ left: base.$el.width(), avoidTransforms: false }, base.options.transition_speed);
                    break;
                case 7: case 'carouselLeft':   // Carousel Left (reverse)
                    nextslide.css({ left: base.$el.width(), 'visibility': 'visible' }).animate({ left: 0, avoidTransforms: false }, base.options.transition_speed, function () { base.afterAnimation(); });
                    liveslide.css({ left: 0 }).animate({ left: -base.$el.width(), avoidTransforms: false }, base.options.transition_speed);
                    break;
            }
            return false;
        };


        /* Play/Pause Toggle
		----------------------------*/
        base.playToggle = function () {

            if (vars.in_animation || !api.options.slideshow) return false;		// Abort if currently animating

            if (vars.is_paused) {

                vars.is_paused = false;

                // Call theme function for play
                if (typeof theme != 'undefined' && typeof theme.playToggle == "function") theme.playToggle('play');

                // Resume slideshow
                vars.slideshow_interval = setInterval(base.nextSlide, base.options.slide_interval);

            } else {

                vars.is_paused = true;

                // Call theme function for pause
                if (typeof theme != 'undefined' && typeof theme.playToggle == "function") theme.playToggle('pause');

                // Stop slideshow
                clearInterval(vars.slideshow_interval);

            }

            return false;

        };

        /* Tear down this instance of supersized
		----------------------------*/
        base.destroy = function (causedByAutoExit) {
            if (vars.in_animation || !api.options.slideshow) return;		// Abort if currently animating

            // Start slideshow if paused. Without this, the slideshow is paused and the play/pause button has the wrong icon
            // when the user clicks the 'start slideshow' button a second time.
            if (vars.is_paused)
                api.playToggle();

            clearInterval(vars.slideshow_interval);

            // Unbind events (requires jQuery 1.7+)
            $(document.documentElement).off('.supersized');
            $('.ssControlsContainer *').off('click');

            var currentSlideId = vars.options.slides[vars.current_slide].id;

            vars = null;
            api = null;

            // Remove slideshow DOM elements and restore the page.
            $('#supersized-loader,#supersized,.ssControlsContainer').remove();
            $('body .supersized_hidden').show().removeClass('supersized_hidden');

            $(window).off('resize');

            // Trigger on_destroy event
            base.options.on_destroy.apply(null, [currentSlideId, causedByAutoExit || false]);
        };

        /* Go to specific slide
	----------------------------*/
        base.goTo = function (targetSlide) {
            if (vars.in_animation || !api.options.slideshow) return false;		// Abort if currently animating

            var totalSlides = base.options.slides.length;

            // If target outside range
            if (targetSlide < 0) {
                targetSlide = totalSlides;
            } else if (targetSlide > totalSlides) {
                targetSlide = 1;
            }
            targetSlide = totalSlides - targetSlide + 1;

            clearInterval(vars.slideshow_interval);	// Stop slideshow, prevent buildup

            // Call theme function for goTo trigger
            if (typeof theme != 'undefined' && typeof theme.goTo == "function") theme.goTo();

            if (vars.current_slide == totalSlides - targetSlide) {
                if (!(vars.is_paused)) {
                    vars.slideshow_interval = setInterval(base.nextSlide, base.options.slide_interval);
                }
                return false;
            }

            // If ahead of current position
            if (totalSlides - targetSlide > vars.current_slide) {

                // Adjust for new next slide
                vars.current_slide = totalSlides - targetSlide - 1;
                vars.update_images = 'next';
                base._placeSlide(vars.update_images);

                //Otherwise it's before current position
            } else if (totalSlides - targetSlide < vars.current_slide) {

                // Adjust for new prev slide
                vars.current_slide = totalSlides - targetSlide + 1;
                vars.update_images = 'prev';
                base._placeSlide(vars.update_images);

            }

            // set active markers
            if (base.options.slide_links) {
                $(vars.slide_list + '> .current-slide').removeClass('current-slide');
                $(vars.slide_list + '> li').eq((totalSlides - targetSlide)).addClass('current-slide');
            }

            if (base.options.thumb_links) {
                $(vars.thumb_list + '> .current-thumb').removeClass('current-thumb');
                $(vars.thumb_list + '> li').eq((totalSlides - targetSlide)).addClass('current-thumb');
            }

        };


        /* Place Slide
----------------------------*/
        base._placeSlide = function (place) {

            // If links should open in new window
            var linkTarget = base.options.new_window ? ' target="_blank"' : '';

            loadSlide = false;

            if (place == 'next') {

                vars.current_slide == base.options.slides.length - 1 ? loadSlide = 0 : loadSlide = vars.current_slide + 1;	// Determine next slide

                var targetList = base.el + ' li:eq(' + loadSlide + ')';

                if (!$(targetList).html()) {
                    // If links should open in new window
                    var linkTarget = base.options.new_window ? ' target="_blank"' : '';

                    imageLink = (base.options.slides[loadSlide].url) ? "href='" + base.options.slides[loadSlide].url + "'" : "";	// If link exists, build it
                    var img = $('<img src="' + base.options.slides[loadSlide].image + '"/>');

                    img.appendTo(targetList).wrap('<a ' + imageLink + linkTarget + '></a>').parent().parent().addClass('image-loading').css('visibility', 'hidden');

                    img.load(function () {
                        base._origDim($(this));
                        base.resizeNow();
                    });	// End Load
                };

                base.nextSlide();

            } else if (place == 'prev') {

                vars.current_slide - 1 < 0 ? loadSlide = base.options.slides.length - 1 : loadSlide = vars.current_slide - 1;	// Determine next slide

                var targetList = base.el + ' li:eq(' + loadSlide + ')';

                if (!$(targetList).html()) {
                    // If links should open in new window
                    var linkTarget = base.options.new_window ? ' target="_blank"' : '';

                    imageLink = (base.options.slides[loadSlide].url) ? "href='" + base.options.slides[loadSlide].url + "'" : "";	// If link exists, build it
                    var img = $('<img src="' + base.options.slides[loadSlide].image + '"/>');

                    img.appendTo(targetList).wrap('<a ' + imageLink + linkTarget + '></a>').parent().parent().addClass('image-loading').css('visibility', 'hidden');

                    img.load(function () {
                        base._origDim($(this));
                        base.resizeNow();
                    });	// End Load
                };
                base.prevSlide();
            }

        };


        /* Get Original Dimensions
		----------------------------*/
        base._origDim = function (targetSlide) {
            targetSlide.data('origWidth', targetSlide.width()).data('origHeight', targetSlide.height());
        };


        /* After Slide Animation
		----------------------------*/
        base.afterAnimation = function () {

            // If hybrid mode is on swap back to higher image quality
            if (base.options.performance == 1) {
                base.$el.removeClass('speed').addClass('quality');
            }

            // Update previous slide
            if (vars.update_images) {
                vars.current_slide - 1 < 0 ? setPrev = base.options.slides.length - 1 : setPrev = vars.current_slide - 1;
                vars.update_images = false;
                $('.prevslide').removeClass('prevslide');
                $(base.el + ' li:eq(' + setPrev + ')').addClass('prevslide');
            }

            vars.in_animation = false;

            // Resume slideshow
            if (!vars.is_paused && base.options.slideshow) {
                vars.slideshow_interval = setInterval(base.nextSlide, base.options.slide_interval);
                if (!base.options.loop && !base.options.auto_exit && vars.current_slide == base.options.slides.length - 1) base.playToggle();
            }

            // Call theme function for after slide transition
            if (typeof theme != 'undefined' && typeof theme.afterAnimation == "function") theme.afterAnimation();

            return false;

        };

        base.getField = function (field) {
            return base.options.slides[vars.current_slide][field];
        };

        // Make it go!
        base.init();
    };


    /* Global Variables
	----------------------------*/
    $.supersized.vars = {

        // Elements							
        thumb_tray: '#thumb-tray',	// Thumbnail tray
        thumb_list: '#thumb-list',	// Thumbnail list
        slide_list: '#slide-list',	// Slide link list

        // Internal variables
        current_slide: 0,			// Current slide number
        in_animation: false,		// Prevents animations from stacking
        is_paused: false,		// Tracks paused on/off
        hover_pause: false,		// If slideshow is paused from hover
        slideshow_interval: false,		// Stores slideshow timer					
        update_images: false,		// Trigger to update images after slide jump
        options: {}			// Stores assembled options list

    };


    /* Default Options
	----------------------------*/
    $.supersized.defaultOptions = {

        // Functionality
        slideshow: 1,			// Slideshow on/off
        autoplay: 1,			// Slideshow starts playing automatically
        auto_exit: 0,      // Exit the slideshow when the last slide is finished
        start_slide: 1,			// Start slide (0 is random)
        loop: 1,			// Enables moving between the last and first slide.
        random: 0,			// Randomize slide order (Ignores start slide)
        slide_interval: 5000,		// Length between transitions
        transition: 1, 			// 0-None, 1-Fade, 2-Slide Top, 3-Slide Right, 4-Slide Bottom, 5-Slide Left, 6-Carousel Right, 7-Carousel Left
        transition_speed: 750,		// Speed of transition
        new_window: 1,			// Image links open in new window/tab
        pause_hover: 0,			// Pause slideshow on hover
        keyboard_nav: 1,			// Keyboard navigation on/off
        performance: 1,			// 0-Normal, 1-Hybrid speed/quality, 2-Optimizes image quality, 3-Optimizes transition speed //  (Only works for Firefox/IE, not Webkit)
        image_protect: 1,			// Disables image dragging and right click with Javascript

        // Size & Position
        fit_always: 0,			// Image will never exceed browser width or height (Ignores min. dimensions)
        fit_landscape: 0,			// Landscape images will not exceed browser width
        fit_portrait: 1,			// Portrait images will not exceed browser height  			   
        min_width: 0,			// Min width allowed (in pixels)
        min_height: 0,			// Min height allowed (in pixels)
        horizontal_center: 1,			// Horizontally center background
        vertical_center: 1,			// Vertically center background


        // Components							
        slide_links: 1,			// Individual links for each slide (Options: false, 'num', 'name', 'blank')
        thumb_links: 1,			// Individual thumb links for each slide
        thumbnail_navigation: 0,			// Thumbnail navigation
        on_destroy: function () { } // Empty implementation for on_destroy event, may be overridden by user

    };

    $.fn.supersized = function (options) {
        return this.each(function () {
            (new $.supersized(options));
        });
    };

})(jQuery);

/*
	supersized.shutter.js
	Supersized - Fullscreen Slideshow jQuery Plugin
	Version : 3.2.7
	Theme 	: Shutter 1.1
	
	Site	: www.buildinternet.com/project/supersized
	Author	: Sam Dunn
	Company : One Mighty Roar (www.onemightyroar.com)
	License : MIT License / GPL License

*/

(function ($) {

    theme = {


        /* Initial Placement
		----------------------------*/
        _init: function () {

            // Configure Slide Links
            if (api.options.slide_links) {
                // Note: This code is repeated in the resize event, so if you change it here do it there, too.
                var maxSlideListWidth = $(vars.slide_list).parent().width() - 400; // Constrain the slide bullets area width so they don't cover buttons
                $(vars.slide_list).css('margin-left', -$(vars.slide_list).width() / 2).css('max-width', maxSlideListWidth);
            }

            // Start progressbar if autoplay enabled
            if (api.options.autoplay) {
                if (api.options.progress_bar) theme.progressBar(); else $(vars.progress_bar).parent().hide();
            } else {
                if ($(vars.play_button).attr('src')) $(vars.play_button).attr("src", api.options.image_path + "play.png");	// If pause play button is image, swap src
                if (api.options.progress_bar)
                    $(vars.progress_bar).stop().css({ left: -$(window).width() });	//  Place progress bar
                else
                    $(vars.progress_bar).parent().hide();
            }


            /* Thumbnail Tray
			----------------------------*/
            // Hide tray off screen
            $(vars.thumb_tray).css({ bottom: -($(vars.thumb_tray).outerHeight() + 5) });

            // Thumbnail Tray Toggle
            $(vars.tray_button).click(function(e) {
                var isExpanded = $(e.currentTarget).data('isExpanded') || false;

                if (isExpanded) {
                    $(vars.thumb_tray).stop().animate({ bottom: -($(vars.thumb_tray).outerHeight() + 5), avoidTransforms: true }, 300);
                    if ($(vars.tray_arrow).attr('src')) $(vars.tray_arrow).attr("src", api.options.image_path + "button-tray-up.png");
                } else {
                    $(vars.thumb_tray).stop().animate({ bottom: 0, avoidTransforms: true }, 300);
                    if ($(vars.tray_arrow).attr('src')) $(vars.tray_arrow).attr("src", api.options.image_path + "button-tray-down.png");
                }
                $(e.currentTarget).data('isExpanded', !isExpanded);
                    
                return false;
            });

            // Make thumb tray proper size
            $(vars.thumb_list).width($('> li', vars.thumb_list).length * $('> li', vars.thumb_list).outerWidth(true));	//Adjust to true width of thumb markers

            // Display total slides
            if ($(vars.slide_total).length) {
                $(vars.slide_total).html(api.options.slides.length);
            }


            /* Thumbnail Tray Navigation
			----------------------------*/
            if (api.options.thumb_links) {
                //Hide thumb arrows if not needed
                if ($(vars.thumb_list).width() <= $(vars.thumb_tray).width()) {
                    $(vars.thumb_back + ',' + vars.thumb_forward).fadeOut(0);
                }

                // Thumb Intervals
                vars.thumb_interval = Math.floor($(vars.thumb_tray).width() / $('> li', vars.thumb_list).outerWidth(true)) * $('> li', vars.thumb_list).outerWidth(true);
                vars.thumb_page = 0;

                // Cycle thumbs forward
                $(vars.thumb_forward).click(function () {
                    if (vars.thumb_page - vars.thumb_interval <= -$(vars.thumb_list).width()) {
                        vars.thumb_page = 0;
                        $(vars.thumb_list).stop().animate({ 'left': vars.thumb_page }, { duration: 500, easing: 'easeOutExpo' });
                    } else {
                        vars.thumb_page = vars.thumb_page - vars.thumb_interval;
                        $(vars.thumb_list).stop().animate({ 'left': vars.thumb_page }, { duration: 500, easing: 'easeOutExpo' });
                    }
                });

                // Cycle thumbs backwards
                $(vars.thumb_back).click(function () {
                    if (vars.thumb_page + vars.thumb_interval > 0) {
                        vars.thumb_page = Math.floor($(vars.thumb_list).width() / vars.thumb_interval) * -vars.thumb_interval;
                        if ($(vars.thumb_list).width() <= -vars.thumb_page) vars.thumb_page = vars.thumb_page + vars.thumb_interval;
                        $(vars.thumb_list).stop().animate({ 'left': vars.thumb_page }, { duration: 500, easing: 'easeOutExpo' });
                    } else {
                        vars.thumb_page = vars.thumb_page + vars.thumb_interval;
                        $(vars.thumb_list).stop().animate({ 'left': vars.thumb_page }, { duration: 500, easing: 'easeOutExpo' });
                    }
                });

            }


            /* Navigation Items
			----------------------------*/
            $(vars.next_slide).click(function () {
                api.nextSlide();
            });

            $(vars.prev_slide).click(function () {
                api.prevSlide();
            });

            // Add touchscreen support for wiping left and right. Requires existence of touchwipe library (http://www.netcu.de/jquery-touchwipe-iphone-ipad-library)
            var isTouchScreen = !!('ontouchstart' in window) || !!navigator.msMaxTouchPoints;
            if (isTouchScreen && $.fn.touchwipe) {
                vars.$container.touchwipe({
                    wipeLeft: function () { api.nextSlide(); },
                    wipeRight: function () { api.prevSlide(); }
                });
            }

            // Full Opacity on Hover
            if (jQuery.support.opacity) {
                $(vars.prev_slide + ',' + vars.next_slide).mouseover(function () {
                    $(this).stop().animate({ opacity: 1 }, 100);
                }).mouseout(function () {
                    $(this).stop().animate({ opacity: 0.6 }, 100);
                });
            }

            if (api.options.thumbnail_navigation) {
                // Next thumbnail clicked
                $(vars.next_thumb).click(function () {
                    api.nextSlide();
                });
                // Previous thumbnail clicked
                $(vars.prev_thumb).click(function () {
                    api.prevSlide();
                });
            }

            $(vars.play_button).click(function () {
                api.playToggle();
            });


            /* Thumbnail Mouse Scrub
			----------------------------*/
            if (api.options.mouse_scrub) {
                $(vars.thumb_tray).mousemove(function (e) {
                    var containerWidth = $(vars.thumb_tray).width(),
						listWidth = $(vars.thumb_list).width();
                    if (listWidth > containerWidth) {
                        var mousePos = 1,
							diff = e.pageX - mousePos;
                        if (diff > 10 || diff < -10) {
                            mousePos = e.pageX;
                            newX = (containerWidth - listWidth) * (e.pageX / containerWidth);
                            diff = parseInt(Math.abs(parseInt($(vars.thumb_list).css('left')) - newX)).toFixed(0);
                            $(vars.thumb_list).stop().animate({ 'left': newX }, { duration: diff * 3, easing: 'easeOutExpo' });
                        }
                    }
                });
            }


            /* Window Resize
			----------------------------*/
            $(window).resize(function () {

                // Delay progress bar on resize
                if (api.options.progress_bar && !vars.in_animation) {
                    if (vars.slideshow_interval) clearInterval(vars.slideshow_interval);
                    if (api.options.slides.length - 1 > 0) clearInterval(vars.slideshow_interval);

                    $(vars.progress_bar).stop().css({ left: -$(window).width() });

                    if (!vars.progressDelay && api.options.slideshow) {
                        // Delay slideshow from resuming so Chrome can refocus images
                        vars.progressDelay = setTimeout(function () {
                            if (!vars.is_paused) {
                                theme.progressBar();
                                vars.slideshow_interval = setInterval(api.nextSlide, api.options.slide_interval);
                            }
                            vars.progressDelay = false;
                        }, 1000);
                    }
                }

                // Thumb Links
                if (api.options.thumb_links && vars.thumb_tray.length) {
                    // Update Thumb Interval & Page
                    vars.thumb_page = 0;
                    vars.thumb_interval = Math.floor($(vars.thumb_tray).width() / $('> li', vars.thumb_list).outerWidth(true)) * $('> li', vars.thumb_list).outerWidth(true);

                    // Adjust thumbnail markers
                    if ($(vars.thumb_list).width() > $(vars.thumb_tray).width()) {
                        $(vars.thumb_back + ',' + vars.thumb_forward).fadeIn('fast');
                        $(vars.thumb_list).stop().animate({ 'left': 0 }, 200);
                    } else {
                        $(vars.thumb_back + ',' + vars.thumb_forward).fadeOut('fast');
                    }

                }

                // Configure Slide Links
                if (api.options.slide_links) {
                    // Note: This code is repeated in the _init function, so if you change it here do it there, too.
                    maxSlideListWidth = $(vars.slide_list).parent().width() - 400; // Constrain the slide bullets area width so they don't cover buttons
                    $(vars.slide_list).css('margin-left', -$(vars.slide_list).width() / 2).css('max-width', maxSlideListWidth);
                }
            });


        },


        /* Go To Slide
		----------------------------*/
        goTo: function () {
            if (api.options.progress_bar && !vars.is_paused) {
                $(vars.progress_bar).stop().css({ left: -$(window).width() });
                theme.progressBar();
            }
        },

        /* Play & Pause Toggle
		----------------------------*/
        playToggle: function (state) {

            if (state == 'play') {
                // If image, swap to pause
                if ($(vars.play_button).attr('src')) $(vars.play_button).attr("src", api.options.image_path + "pause.png");
                if (api.options.progress_bar && !vars.is_paused) theme.progressBar();
            } else if (state == 'pause') {
                // If image, swap to play
                if ($(vars.play_button).attr('src')) $(vars.play_button).attr("src", api.options.image_path + "play.png");
                if (api.options.progress_bar && vars.is_paused) $(vars.progress_bar).stop().css({ left: -$(window).width() });
            }

        },


        /* Before Slide Transition
		----------------------------*/
        beforeAnimation: function (direction) {
            if (api.options.progress_bar && !vars.is_paused) $(vars.progress_bar).stop().css({ left: -$(window).width() });

            /* Update Fields
			----------------------------*/
            // Update slide caption
            if ($(vars.slide_caption).length) {
                (api.getField('title')) ? $(vars.slide_caption).html(api.getField('title')) : $(vars.slide_caption).html('');
            }
            // Update slide number
            if (vars.slide_current.length) {
                $(vars.slide_current).html(vars.current_slide + 1);
            }


            // Highlight current thumbnail and adjust row position
            if (api.options.thumb_links) {

                $('.current-thumb').removeClass('current-thumb');
                $('li', vars.thumb_list).eq(vars.current_slide).addClass('current-thumb');

                // If thumb out of view
                if ($(vars.thumb_list).width() > $(vars.thumb_tray).width()) {
                    // If next slide direction
                    if (direction == 'next') {
                        if (vars.current_slide == 0) {
                            vars.thumb_page = 0;
                            $(vars.thumb_list).stop().animate({ 'left': vars.thumb_page }, { duration: 500, easing: 'easeOutExpo' });
                        } else if ($('.current-thumb').offset().left - $(vars.thumb_tray).offset().left >= vars.thumb_interval) {
                            vars.thumb_page = vars.thumb_page - vars.thumb_interval;
                            $(vars.thumb_list).stop().animate({ 'left': vars.thumb_page }, { duration: 500, easing: 'easeOutExpo' });
                        }
                        // If previous slide direction
                    } else if (direction == 'prev') {
                        if (vars.current_slide == api.options.slides.length - 1) {
                            vars.thumb_page = Math.floor($(vars.thumb_list).width() / vars.thumb_interval) * -vars.thumb_interval;
                            if ($(vars.thumb_list).width() <= -vars.thumb_page) vars.thumb_page = vars.thumb_page + vars.thumb_interval;
                            $(vars.thumb_list).stop().animate({ 'left': vars.thumb_page }, { duration: 500, easing: 'easeOutExpo' });
                        } else if ($('.current-thumb').offset().left - $(vars.thumb_tray).offset().left < 0) {
                            if (vars.thumb_page + vars.thumb_interval > 0) return false;
                            vars.thumb_page = vars.thumb_page + vars.thumb_interval;
                            $(vars.thumb_list).stop().animate({ 'left': vars.thumb_page }, { duration: 500, easing: 'easeOutExpo' });
                        }
                    }
                }


            }

        },


        /* After Slide Transition
		----------------------------*/
        afterAnimation: function () {
            if (api.options.progress_bar && !vars.is_paused) theme.progressBar();	//  Start progress bar
        },


        /* Progress Bar
		----------------------------*/
        progressBar: function () {
            $(vars.progress_bar).stop().css({ left: -$(window).width() }).animate({ left: 0 }, api.options.slide_interval);
        }


    };


    /* Theme Specific Variables
	----------------------------*/
    $.supersized.themeVars = {

        // Internal Variables
        progress_delay: false,				// Delay after resize before resuming slideshow
        thumb_page: false,				// Thumbnail page
        thumb_interval: false,				// Thumbnail interval

        // General Elements							
        play_button: '#pauseplay',		// Play/Pause button
        next_slide: '#nextslide',		// Next slide button
        prev_slide: '#prevslide',		// Prev slide button
        next_thumb: '#nextthumb',		// Next slide thumb button
        prev_thumb: '#prevthumb',		// Prev slide thumb button

        slide_caption: '#slidecaption',	// Slide caption
        slide_current: '.slidenumber',		// Current slide number
        slide_total: '.totalslides',		// Total Slides
        slide_list: '#slide-list',		// Slide jump list							

        thumb_tray: '#thumb-tray',		// Thumbnail tray
        thumb_list: '#thumb-list',		// Thumbnail list
        thumb_forward: '#thumb-forward',	// Cycles forward through thumbnail list
        thumb_back: '#thumb-back',		// Cycles backwards through thumbnail list
        tray_arrow: '#tray-arrow',		// Thumbnail tray button arrow
        tray_button: '#tray-button',		// Thumbnail tray button

        progress_bar: '#progress-bar'		// Progress bar

    };

    /* Theme Specific Options
	----------------------------*/
    $.supersized.themeOptions = {

        progress_bar: 1,		// Timer for each slide											
        image_path: 'img/',				// Default image path
        mouse_scrub: 0,		// Thumbnails move with mouse
        // html_template contains the HTML for the slideshow controls
        html_template: '<div class="ssControlsContainer"> \
		<!--Thumbnail Navigation--> \
		<div id="prevthumb"></div> \
		<div id="nextthumb"></div> \
\
		<!--Arrow Navigation--> \
		<a id="prevslide" class="load-item"></a> \
		<a id="nextslide" class="load-item"></a> \
\
		<div id="thumb-tray" class="load-item"> \
			<div id="thumb-back"></div> \
			<div id="thumb-forward"></div> \
		</div> \
\
		<!--Time Bar--> \
		<div id="progress-back" class="load-item"> \
			<div id="progress-bar"></div> \
		</div> \
\
		<!--Control Bar--> \
		<div id="controls-wrapper" class="load-item"> \
			<div id="controls"> \
\
				<a id="play-button"> \
					<img id="pauseplay" src="img/pause.png" /></a> \
\
				<a id="stop-button"> \
					<img src="img/stop.png" /></a> \
\
				<!--Slide counter--> \
				<div id="slidecounter"> \
					<span class="slidenumber"></span>/ <span class="totalslides"></span> \
				</div> \
\
				<!--Slide captions displayed here--> \
				<div id="slidecaption"></div> \
\
				<!--Thumb Tray button--> \
				<a id="tray-button"> \
					<img id="tray-arrow" src="img/button-tray-up.png" /></a> \
\
				<!--Navigation--> \
				<ul id="slide-list"></ul> \
\
			</div> \
		</div> \
</div>'

    };


})(jQuery);

//#endregion End supersized
