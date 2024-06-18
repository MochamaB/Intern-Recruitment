(function ($) {
    'use strict';
    $(document).ready(function () {
        // Start the progress bar as soon as the document is ready
        startProgressBar();

        // Stop the progress bar once everything is loaded
        $(window).on('load', function () {
            stopProgressBar();
        });

    var progressInterval;
   // alert("progress");

    $(document).ajaxStart(function () {
        startProgressBar();
    });

    $(document).ajaxSend(function (event, xhr, options) {
        updateProgressBar();
    });

    $(document).ajaxStop(function () {
        stopProgressBar();
    });

    window.onbeforeunload = function () {
        startProgressBar();
    };

    window.onload = function () {
        stopProgressBar();
    };

    function startProgressBar() {
        $("#progress-bar").css("width", "0%");
        $("#progress-container").show();
        updateProgressBar();
    }

    function updateProgressBar() {
        clearInterval(progressInterval);
        var percentage = 0;
        progressInterval = setInterval(function () {
            percentage += 1;
            $("#progress-bar").css("width", percentage + "%");

            if (percentage >= 100) {
                clearInterval(progressInterval);
            }
        }, 50);
    }

    function stopProgressBar() {
        clearInterval(progressInterval);
        $("#progress-container").hide();
    }
});
})(jQuery);