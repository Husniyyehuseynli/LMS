// Handles clicking a lesson's status icon on the Course Details "Curriculum"
// tab: toggles completed/not-completed via AJAX, then updates that icon and
// the overall progress bar without reloading the page.
document.addEventListener('DOMContentLoaded', function () {
    var buttons = document.querySelectorAll('.lesson-progress-toggle');

    buttons.forEach(function (button) {
        button.addEventListener('click', function () {
            var lessonId = button.getAttribute('data-lesson-id');

            fetch('/LessonProgress/Toggle', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: 'lessonId=' + encodeURIComponent(lessonId)
            })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    updateLessonIcon(button, data.isCompleted);
                    updateProgressBar(data.percent);

                    if (data.isCompleted && data.percent === 100 && window.lmsToast) {
                        window.lmsToast('🎉 Great job — every lesson in this course is complete!', 'success');
                    } else if (data.isCompleted && window.lmsToast) {
                        window.lmsToast('Lesson marked as completed.', 'success');
                    }
                })
                .catch(function () {
                    // Silently ignore — the icon just stays as it was.
                });
        });
    });

    function updateLessonIcon(button, isCompleted) {
        var icon = button.querySelector('.lesson-status-icon');
        var row = button.closest('.curriculum-item');

        if (isCompleted) {
            icon.classList.remove('fa-circle', 'text-muted');
            icon.classList.add('fa-check-circle', 'text-success');
            button.title = 'Mark as not started';
            if (row) row.classList.add('curriculum-item-completed');
        } else {
            icon.classList.remove('fa-check-circle', 'text-success');
            icon.classList.add('fa-circle', 'text-muted');
            button.title = 'Mark as completed';
            if (row) row.classList.remove('curriculum-item-completed');
        }
    }

    function updateProgressBar(percent) {
        var bar = document.getElementById('lessonProgressBar');
        var text = document.getElementById('lessonProgressText');

        if (bar) {
            bar.style.width = percent + '%';
            bar.setAttribute('aria-valuenow', percent);
        }
        if (text) {
            text.textContent = percent + '% complete';
        }
    }
});
