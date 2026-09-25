/* UniSkill Hub - quiz page behaviour (plain JavaScript).
   Everything here is a convenience for the student. The real time limit and the
   scoring are enforced on the server (TakeQuiz.aspx.cs). */
(function () {
    'use strict';

    var timerEl, answeredEl, submitBtn;
    var remaining = 0;
    var autoSubmitting = false;

    function pad(n) { return (n < 10 ? '0' : '') + n; }

    function showTime() {
        var minutes = Math.floor(remaining / 60);
        var seconds = remaining % 60;
        timerEl.textContent = pad(minutes) + ':' + pad(seconds);
        timerEl.classList.toggle('is-low', remaining <= 60);
    }

    // Counts the questions that have a chosen option
    function updateAnswered() {
        var questions = document.querySelectorAll('.quiz-question');
        var count = 0;
        for (var i = 0; i < questions.length; i++) {
            if (questions[i].querySelector('input[type="radio"]:checked')) { count++; }
        }
        answeredEl.textContent = count;
        return { answered: count, total: questions.length };
    }

    function tick() {
        remaining = Math.max(0, remaining - 1);
        showTime();

        if (remaining === 0 && !autoSubmitting) {
            // Time is up: submit whatever has been answered so far.
            autoSubmitting = true;
            submitBtn.click();
        }
    }

    // Used as OnClientClick of the Submit button: warns about unanswered questions.
    window.confirmQuizSubmit = function () {
        if (autoSubmitting) { return true; }

        var status = updateAnswered();
        if (status.answered < status.total) {
            var left = status.total - status.answered;
            return window.confirm('You have ' + left + ' unanswered ' + (left === 1 ? 'question' : 'questions') +
                                  '. Submit the quiz anyway?');
        }
        return true;
    };

    document.addEventListener('DOMContentLoaded', function () {
        timerEl = document.querySelector('[data-quiz-timer]');
        answeredEl = document.querySelector('[data-quiz-answered]');
        submitBtn = document.querySelector('[data-quiz-submit]');
        if (!timerEl || !answeredEl || !submitBtn) { return; }

        remaining = parseInt(timerEl.getAttribute('data-remaining'), 10) || 0;
        showTime();
        updateAnswered();

        document.addEventListener('change', function (e) {
            if (e.target && e.target.type === 'radio') { updateAnswered(); }
        });

        window.setInterval(tick, 1000);
    });
})();
