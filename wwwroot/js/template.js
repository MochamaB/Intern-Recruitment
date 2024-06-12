(function ($) {
    'use strict';
    $(document).ready(function () {
        var sidebar = $('.sidebar');

        // Function to add active class based on the current URL
        function addActiveClass(element) {
            var currentPath = location.pathname.split("/")[1]; // Get the first part of the URL path
            var linkPath = element.attr('href').split("/")[1]; // Get the first part of the href

            if (currentPath === linkPath) {
                element.parents('.nav-item').last().addClass('active');
                if (element.parents('.sub-menu').length) {
                    element.closest('.collapse').addClass('show');
                    element.addClass('active');
                }
                if (element.parents('.submenu-item').length) {
                    element.addClass('active');
                }
            } else if (currentPath.startsWith("RequisitionWizard") && linkPath === "Requisitions") {
                // Special case for RequisitionWizard
                element.parents('.nav-item').last().addClass('active');
                if (element.parents('.sub-menu').length) {
                    element.closest('.collapse').addClass('show');
                    element.addClass('active');
                }
                if (element.parents('.submenu-item').length) {
                    element.addClass('active');
                }
            }
        }

        // Iterate over each sidebar link and apply the active class
        $('.nav li a', sidebar).each(function () {
            var $this = $(this);
            addActiveClass($this);
        });

        // Close other submenu in sidebar on opening any
        sidebar.on('show.bs.collapse', '.collapse', function () {
            sidebar.find('.collapse.show').collapse('hide');
        });
    });
})(jQuery);
