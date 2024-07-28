(function ($) {
    'use strict';

        if (typeof jQuery == 'undefined') {
            console.log('jQuery is not loaded');
        } else {
            $(document).ready(function () {
                console.log("Script section loaded");

                // Use event delegation to attach event handler
                $(document).on("click", ".document-link", function (e) {
                    e.preventDefault();
                    console.log("Link clicked");

                    var documentName = $(this).data('document-name');

                    // Set the modal title and body content
                    $('#documentModalLabel').text(documentName);

                    var documentUrl = $(this).data('document-url');
                    console.log(documentUrl);

                    $('#documentViewer').attr('src', documentUrl);
                });
            });
        }
})(jQuery);