/* UniSkill Hub - admin form behaviour (plain JavaScript).
   Shows only the fields that make sense for the chosen resource type.
   The server still decides what is saved; this only keeps the form tidy. */
(function () {
    'use strict';

    function updateTypeFields() {
        var select = document.querySelector('.js-resource-type');
        if (!select) { return; }

        var groups = document.querySelectorAll('[data-for-types]');
        for (var i = 0; i < groups.length; i++) {
            var types = groups[i].getAttribute('data-for-types').split(' ');
            groups[i].hidden = (types.indexOf(select.value) < 0);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var select = document.querySelector('.js-resource-type');
        if (!select) { return; }
        select.addEventListener('change', updateTypeFields);
        updateTypeFields();
    });
})();
