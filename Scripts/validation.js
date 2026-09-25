/* UniSkill Hub - client-side form validation helpers (plain JavaScript).
   These give students instant feedback in the browser. They are only a convenience:
   the server repeats every check, because client-side validation can be bypassed. */
(function () {
    'use strict';

    // Keep these two values in sync with FileHelper.cs (SubmissionExtensions / MaxSubmissionBytes).
    var ALLOWED_EXTENSIONS = ['.pdf', '.doc', '.docx', '.ppt', '.pptx', '.zip', '.txt'];
    var MAX_BYTES = 10 * 1024 * 1024;   // 10 MB

    function fail(validator, args, message) {
        validator.errormessage = message;   // used by the ValidationSummary
        validator.innerHTML = message;      // shown next to the field
        args.IsValid = false;
    }

    // ---- Admin resource uploads (keep in sync with ManageResources.aspx.cs) ----
    var RESOURCE_RULES = {
        document: { type: 'PDF',   extensions: ['.pdf', '.doc', '.docx', '.ppt', '.pptx', '.xls', '.xlsx', '.zip', '.txt'], maxBytes: 20 * 1024 * 1024, label: '20 MB' },
        video:    { type: 'Video', extensions: ['.mp4', '.webm', '.ogv'],                                                    maxBytes: 40 * 1024 * 1024, label: '40 MB' },
        audio:    { type: 'Audio', extensions: ['.mp3', '.ogg', '.wav'],                                                     maxBytes: 20 * 1024 * 1024, label: '20 MB' }
    };

    // Checks the file chosen in the input marked data-file-kind="<kind>", but only while the
    // matching resource type is selected (the other file inputs are hidden).
    function checkResourceFile(validator, args, kind) {
        var rule = RESOURCE_RULES[kind];
        var typeSelect = document.querySelector('.js-resource-type');
        var input = document.querySelector('input[data-file-kind="' + kind + '"]');

        args.IsValid = true;
        if (!rule || !typeSelect || !input || typeSelect.value !== rule.type) { return; }
        if (!input.files || input.files.length === 0) { return; }   // "file required" is checked on the server

        var file = input.files[0];
        var name = file.name.toLowerCase();
        var dot = name.lastIndexOf('.');
        var extension = dot >= 0 ? name.substring(dot) : '';

        if (rule.extensions.indexOf(extension) < 0) {
            fail(validator, args, 'That file type is not allowed. Allowed: ' + rule.extensions.join(', ').toUpperCase() + '.');
        } else if (file.size === 0) {
            fail(validator, args, 'That file is empty.');
        } else if (file.size > rule.maxBytes) {
            fail(validator, args, 'That file is larger than ' + rule.label + '.');
        }
    }

    window.validateDocumentFile = function (v, a) { checkResourceFile(v, a, 'document'); };
    window.validateVideoFile    = function (v, a) { checkResourceFile(v, a, 'video'); };
    window.validateAudioFile    = function (v, a) { checkResourceFile(v, a, 'audio'); };

    // ---- Admin grading form (keep in sync with ManageSubmissions.aspx.cs) ----
    // Marks are only required while the status is "Graded"; they must be a number with at most
    // 2 decimals and not more than the assignment's maximum (stored in the box's data-max).
    window.validateGradeMarks = function (validator, args) {
        var status = document.querySelector('.js-grade-status');
        var box = document.querySelector('.js-grade-marks');
        args.IsValid = true;
        if (!status || !box || status.value !== 'Graded') { return; }

        var text = (args.Value || '').replace(/^\s+|\s+$/g, '');
        var max = parseFloat(box.getAttribute('data-max'));

        if (text === '') {
            fail(validator, args, 'Enter the marks to save a grade.');
        } else if (!/^\d{1,3}(\.\d{1,2})?$/.test(text)) {
            fail(validator, args, 'Marks must be a number such as 85 or 85.5 (up to 2 decimals).');
        } else if (!isNaN(max) && parseFloat(text) > max) {
            fail(validator, args, 'Marks cannot be more than the maximum of ' + max + '.');
        }
    };

    // Used as ClientValidationFunction of the CustomValidator on the assignment FileUpload.
    window.validateSubmissionFile = function (validator, args) {
        var input = document.getElementById(validator.controltovalidate);

        // "No file chosen" is reported by the RequiredFieldValidator, not here.
        if (!input || !input.files || input.files.length === 0) {
            args.IsValid = true;
            return;
        }

        var file = input.files[0];
        var name = file.name.toLowerCase();
        var dot = name.lastIndexOf('.');
        var extension = dot >= 0 ? name.substring(dot) : '';

        if (ALLOWED_EXTENSIONS.indexOf(extension) < 0) {
            fail(validator, args, 'That file type is not allowed. Please upload a PDF, DOC, DOCX, PPT, PPTX, ZIP or TXT file.');
        } else if (file.size === 0) {
            fail(validator, args, 'That file is empty.');
        } else if (file.size > MAX_BYTES) {
            fail(validator, args, 'That file is larger than 10 MB. Please upload a smaller file.');
        } else {
            args.IsValid = true;
        }
    };
})();
