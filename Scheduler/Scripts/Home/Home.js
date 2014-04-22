$(document).ready(function () {
    Home.Init();
});

var Home = new function () {
    this.Init = function () {
        $("#FeedbackOpenClose").click(function () {
            if ($("#FeedbackContainer").hasClass("hidden")) {
                $("#FeedbackContainer").removeClass("hidden");
            } else {
                $("#FeedbackContainer").addClass("hidden");
            }
        });
        $("#FCFSOpenClose").click(function () {
            if ($("#FCFSContainer").hasClass("hidden")) {
                $("#FCFSContainer").removeClass("hidden");
            } else {
                $("#FCFSContainer").addClass("hidden");
            }
        });
        $("#SPNOpenClose").click(function () {
            if ($("#SPNContainer").hasClass("hidden")) {
                $("#SPNContainer").removeClass("hidden");
            } else {
                $("#SPNContainer").addClass("hidden");
            }
        });
        $("#STROpenClose").click(function () {
            if ($("#STRContainer").hasClass("hidden")) {
                $("#STRContainer").removeClass("hidden");
            } else {
                $("#STRContainer").addClass("hidden");
            }
        });
        $("#RROpenClose").click(function () {
            if ($("#RRContainer").hasClass("hidden")) {
                $("#RRContainer").removeClass("hidden");
            } else {
                $("#RRContainer").addClass("hidden");
            }
        });

        $("#NumProcessSubmit").click(function () {
            var input = $("#NumProcesses").val();
            if (isNaN(input)) {
                alert("Please give a valid number 2-7");
            } else {
                if (input < 2 || input > 7) {
                    alert("Please give a valid number 2-7");
                } else {
                    var url = $(location).attr('href');
                    var newUrl = $(location).attr('href').substring(0, url.indexOf('&'));
                    window.location.href = newUrl + "?id=" + input
                    ;
                }
            }
        });
    };
};