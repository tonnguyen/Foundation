import * as $ from 'jquery';
import "jqcloud2";

export default class IdioAnalyseBlock {
    constructor() {
        this.mouseIn = true;
    }

    init() {
        var self = this;

        $(document).on('click', '.idio-analyse-toggler', function () {
            if (self.isContentOpen())
                self.hideContent();
            else
                self.showContent();
        })

        $(document).on('click', '.idio-analyse-collapse-btn', function () {
            self.hideContent();
        })

        $(document).on('mouseenter', '.idio-analyse-content', function () {
            self.mouseIn = true;
        })
        $(document).on('mouseleave', '.idio-analyse-content', function () {
            self.mouseIn = false;
        })

        $(document).on('mouseenter', '.idio-analyse-toggler', function () {
            self.mouseIn = true;
        })
        $(document).on('mouseleave', '.idio-analyse-toggler', function () {
            self.mouseIn = false;
        })

        $(window).on('click', function (e) {
            if (!self.mouseIn)
                self.hideContent();
        })
    }

    isContentOpen() {
        return $('.idio-analyse-content').hasClass('open');
    }

    showContent() {
        this.showOverlay();
        $('.idio-analyse-content').addClass('open');
        $('.idio-analyse-content').show();
        $('.idio-analyse-toggler').css("display", "none");
        this.showTopicsCloud();
    }

    hideContent() {
        this.hideOverlay();
        $('.idio-analyse-content').removeClass('open');
        $('.idio-analyse-content').hide();
        setTimeout(function () {
            $('.idio-analyse-toggler').css("display", "flex");
        }, 200)
    }

    showOverlay() {
        $('.idio-analyse-overlay').width("auto");
        $('.idio-analyse-overlay').height("auto");
    }

    hideOverlay() {
        $('.idio-analyse-overlay').width(0);
        $('.idio-analyse-overlay').height(0);
    }

    showTopicsCloud() {
        $('.topics-cloud').jQCloud(idioTopics.map(o => ({
            text: o.Title,
            weight: o.Weight
        })));
    }
}