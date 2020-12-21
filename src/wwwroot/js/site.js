// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
function handleFilter(source) {
    var filterButton = document.getElementById("FilterButton");
    if (source.value < source.min || source.value > source.max) {
        filterButton.disabled = true;
    }
    else {
        filterButton.disabled = false;
    }
}

function toggleDelete() {
    var deleteButton = document.getElementById("DeleteButton");
    deleteButton.disabled = true;
    var checkboxes = document.getElementsByName("chkEntry");
    for (var i = 0, n = checkboxes.length; i < n; i++) {
        if (checkboxes[i].checked === true) {
            deleteButton.disabled = false;
            break;
        }
    }
}

function toggle(source) {
    var deleteButton = document.getElementById("DeleteButton");
    var anyChecked = false;
    var checkboxes = document.getElementsByName("chkEntry");
    for (var i = 0, n = checkboxes.length; i < n; i++) {
        checkboxes[i].checked = source.checked;
        if (checkboxes[i].checked === true) {
            anyChecked = true;
        }
    }
    if (anyChecked) {
        deleteButton.disabled = false;
    } else {
        deleteButton.disabled = true;
    }
}
