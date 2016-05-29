(function () {

    // set height to the wrapper of the home page sliders and banners so that they are aligned all the same in height
    function resizeSliders() {
        var mainSliderHeight = $('.slider-left').height();
        if (mainSliderHeight != null && mainSliderHeight > 0) {
            if (sevenSpikes.getViewPort().width > 768) {
                $('.home-page-top-banner-wrapper').height(mainSliderHeight);
            }
            else {
                $('.home-page-top-banner-wrapper').css('height', '');
            }
        }
    }

    // resize the sliders when they are actually loaded
    $('.slider-left .slider-wrapper .nivo-main-image').livequery(function () {
        // if we have only one main slider make it 100% wide
        if ($('.banners-right .slider-wrapper').length === 0) {
            $('.slider-left').css('width', '100%');
        }
        else {
            resizeSliders();
        }
    });

    $(document).ready(function () {
        var responsiveAppSettings = {
            isEnabled: true,
            themeBreakpoint: 980,
            isSearchBoxDetachable: true,
            isHeaderLinksWrapperDetachable: true,
            doesDesktopHeaderMenuStick: false,
            doesScrollAfterFiltration: true,
            doesSublistHasIndent: true,
            displayGoToTop: true,
            hasStickyNav: true,
            selectors: {
                menuTitle: ".menu-title",
                headerMenu: ".header-menu",
                closeMenu: ".close-menu",
                movedElements: ".admin-header-links, .h-wrapper, .header, .responsive-nav-wrapper, .master-wrapper-content, .footer",
                sublist: ".header-menu .sublist",
                overlayOffCanvas: ".overlayOffCanvas",
                withSubcategories: ".with-subcategories",
                filtersContainer: ".nopAjaxFilters7Spikes",
                filtersOpener: ".filters-button span",
                searchBoxOpener: ".search-wrap > span",
                searchBox: ".search-box.store-search-box",
                searchBoxBefore: ".header-selectors-wrapper",
                navWrapper: ".responsive-nav-wrapper",
                navWrapperParent: ".responsive-nav-wrapper-parent",
                headerLinksOpener: "#header-links-opener",
                headerLinksWrapper: ".header-links-wrapper",
                shoppingCartLink: ".shopping-cart-link",
                overlayEffectDelay: 300
            }
        };

        // CUSTOM SELECTS
        $(".product-selectors select").simpleSelect();

        sevenSpikes.initResponsiveTheme(responsiveAppSettings);

        // resize the slider wrapper height on resize and orientation changed
        sevenSpikes.addWindowEvent("resize", resizeSliders);
        sevenSpikes.addWindowEvent("orientationchange", resizeSliders);

        // slider-left & banners-right same height
        $(window).load(function () {
            $('.variant-overview > span').click(function () {
                $(this).siblings('.details-wrapper').toggle();
            });

            // quick tab set min height to content container
            var productTabsHeader = $('.productTabs-header').height();
            $('.ui-tabs-panel').css('min-height', productTabsHeader + 'px');

            // instant search custom categories
            $('#instant-search-categories').each(function () {
                var that = $(this);

                that.wrap('<div class="custom-select" />');
                that.parent('.custom-select').prepend('<div class="custom-select-text" />');
                that.siblings('.custom-select-text').text(that.children('option:selected').text());
            }).change(function () {
                $(this).siblings('.custom-select-text').text($(this).children('option:selected').text());
            });

        });
    });
})();