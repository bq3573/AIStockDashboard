//<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
$(document).ready(function () {
    // When the button is clicked


    $("#tickerInput").on("input", function () {
        let selectedValue = $(this).val();
        $.ajax({
            url: "/api/stock/GetTickerFilters",
            method: "GET",
            data: { filter: selectedValue },
            dataType: "text",
            success: function (response) {
                $("#tickerFilter").empty();

                // Populate the response container with the stock data
                let stockInfo = ``;

                $("#tickerFilter").append(response);
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                $("#responseContainer").html("An error occurred: " + error);
            }
        });
    });




    $("#tickerButton").click(function () {
        var ticker = $("#tickerInput").val();  // Get the ticker from the input

        if (!ticker) {
            alert("Please enter a ticker symbol");
            return;
        }

        // Perform the AJAX request
        $.ajax({
            url: "/api/stock/analyze",
            method: "GET",
            data: { ticker: ticker },
            dataType: "text",
            success: function (response) {
                $("#outlookSummary").empty();

                // Populate the response container with the stock data
                let summary = `
                            <div class="card">
                                <div class="card-header">
                                    <h5>Short Term Outlook</h5>
                                </div>
                                <div class="card-body">
                                    <p>${response}</p>
                                </div>
                            </div>
                                    </br>
                        `;

                $("#outlookSummary").append(summary);

            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                $("#outlookSummary").html("An error occurred: " + error);
            }
        });
    });



});