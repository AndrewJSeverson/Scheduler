$(document).ready(function() {
    Home.Init();
});

var Home = new function() {
    this.Init = function() {
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
    };
};