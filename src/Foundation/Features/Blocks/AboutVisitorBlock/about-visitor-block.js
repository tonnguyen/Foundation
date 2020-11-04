import * as $ from 'jquery';

export default class AboutVisitorBlock {
    constructor() {
        this.contentWidth = 0;
        this.mouseIn = true;
    }

    init() {
        var self = this;
        this.contentWidth = $('.about-visitor-content').data('width');
        if (this.contentWidth)
            this.hideContent();

        $(document).on('click', '.about-visitor-toggler', function () {
            if (self.isContentOpen())
                self.hideContent();
            else
                self.showContent();
        })

        $(document).on('click', '.about-visitor-collapse-btn', function () {
            self.hideContent();
        })

        $(document).on('mouseenter', '.about-visitor-container', function () {
            self.mouseIn = true;
        })
        $(document).on('mouseleave', '.about-visitor-container', function () {
            self.mouseIn = false;
        })

        $(window).on('click', function (e) {
            if (!self.mouseIn)
                self.hideContent();
        })
    }

    isContentOpen() {
        return $('.about-visitor-overlay').hasClass('open');
    }

    showContent() {
        this.showOverlay();
        $('.about-visitor-container').addClass('open');
        $('.about-visitor-container').css("right", 0);
        $('.about-visitor-toggler').css("display", "none");
    }

    hideContent() {
        this.hideOverlay();
        $('.about-visitor-container').removeClass('open');
        $('.about-visitor-container').css("right", -(parseInt(this.contentWidth) + 3));
        setTimeout(function () {
            $('.about-visitor-toggler').css("display", "flex");
        }, 200)
    }

    showOverlay() {
        $('.about-visitor-overlay').width("auto");
        $('.about-visitor-overlay').height("auto");
    }

    hideOverlay() {
        $('.about-visitor-overlay').width(0);
        $('.about-visitor-overlay').height(0);
    }
}