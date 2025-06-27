//<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
$(document).ready(function () {
    // When the button is clicked

    // Delegate event to handle dynamically added buttons
    $(document).on("click", ".copy-btn", function () {
        const url = $(this).data("url");

        navigator.clipboard.writeText(url).then(() => {
            $(this).text("Copied!");
            setTimeout(() => $(this).text("Copy"), 1500);
        }).catch(err => {
            console.error("Failed to copy: ", err);
            $(this).text("Error");
        });
    });

    //GetTrendingStocks = function () {
    //    // Hide container initially
    //    $("#trendingContainer").hide();

    //    $.ajax({
    //        url: "/api/stock/Trending",
    //        method: "GET",
    //        success: function (data) {
    //            if (!Array.isArray(data) || data.length === 0) {
    //                $("#trendingContainer").html(""); // remain hidden
    //                return;
    //            }

    //            let html = `
    //            <div class="trending-header">🔥 Trending Stocks</div>
    //            <ul class="trending-list">
    //                ${data.map(t => `
    //                    <li class="trending-item">
    //                        🔥 <strong>${t.ticker}</strong>
    //                        <span class="change-percent">(${t.dp.toFixed(2)}%)</span>
    //                    </li>
    //                `).join("")}
    //            </ul>
    //        `;

    //            $("#trendingContainer").html(html).show();
    //        },
    //        error: function () {
    //            $("#trendingContainer").html("<p class='text-danger'>Failed to load trending stocks.</p>").show();
    //            $("#trendingContainer").hide();
    //        }
    //    });
    //};


    

    $("#newsButton").click(function () {
        var url = $("#urlInput").val();
        $.ajax({
            url: "/api/stock/summary",
            method: "GET",
            data: { url: url },
            dataType: "text",
            success: function (response) {
                //Console.log(response);
                $("#responseContainerSummary").empty();

                let summary = `
                    <div class="summary-card">
                        <div class="summary-card-header">
                            📝 <span>Article Summary</span>
                        </div>
                        <div class="summary-card-body">
                            <p>${response}</p>
                        </div>
                    </div>
                    <br/>
                `;

                $("#responseContainerSummary").append(summary);



            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                $("#responseContainerTwo").html("An error occurred: " + error);
            }
        });
    });



    StockSearch = function (ticker) {
        $.ajax({
            url: "/api/stock/search",
            method: "GET",
            data: { ticker: ticker },
            dataType: "json",
            success: function (response) {
                $("#responseContainer").empty();

                let isNegative = parseFloat(response.change) < 0;
                let changeClass = isNegative ? "negative" : "positive";

                let stockInfo = `
                    <div class="card stock-card">
                        <div class="card-header">
                            <h5>${response.symbol} – Latest Stock Information</h5>
                        </div>
                        <div class="card-body">
                            <p><strong>Open:</strong> ${response.open}</p>
                            <p><strong>High:</strong> ${response.high}</p>
                            <p><strong>Low:</strong> ${response.low}</p>
                            <p><strong>Price:</strong> ${response.price}</p>
                            <p><strong>Volume:</strong> ${response.volume}</p>
                            <p><strong>Latest Trading Day:</strong> ${response.latestTradingDay}</p>
                            <p><strong>Previous Close:</strong> ${response.previousClose}</p>
                            <p><strong>Change:</strong> ${response.change}</p>
                            <p><strong>Change Percent:</strong> <span class="${changeClass}">${response.changePercent}</span></p>
                        </div>
                    </div>
                    <br/>
                `;

                if (response.negOrPos) {
                    $('#themeStylesheet').attr('href', `/css/GreenTheme.css`);
                } else {
                    $('#themeStylesheet').attr('href', `/css/RedTheme.css`);
                }

                $("#responseContainer").append(stockInfo);

            },
            error: function (xhr, status, error) {
                $("#responseContainer").empty();

                let errorCard = `
                    <div class="card border-danger mb-3">
                        <div class="card-header bg-danger text-white">
                            <h5>⚠️ Error: API Limit Reached</h5>
                        </div>
                        <div class="card-body">
                            <p>You've reached the maximum number of API requests (25) allowed for now.</p>
                            <p>Please wait before trying again or consider upgrading your API plan if applicable.</p>
                            <p class="text-muted">Technical Info: ${error}</p>
                        </div>
                    </div>
                `;

                $("#responseContainer").append(errorCard);
            }
        });
    }

    GetNews = function (ticker) {
        $.ajax({
            url: "/api/stock/news",
            method: "GET",
            data: { ticker: ticker },
            dataType: "json",
            success: function (response) {
                $("#responseContainerTwo").empty();

                response.forEach(news => {
                    const stockInfo = `
                        <div class="news-card">
                            <div class="news-card-header">
                                📰 <span>${news.title}</span>
                            </div>
                            <div class="news-card-body">
                                <p><strong>Source:</strong> ${news.source}</p>
                                <p><strong>Published:</strong> ${news.publishedDate}</p>
                                <p><strong>Sentiment:</strong> ${news.sentimentLabel}</p>
                                <p><strong>Sentiment Score:</strong> ${news.sentimentScore}</p>
                                <div class="news-url">
                                    🔗 <a href="${news.url}" target="_blank">${news.url}</a>
                                    <button class="copy-btn" data-url="${news.url}">Copy</button>
                                </div>
                            </div>
                        </div>
                        <br/>
                    `;
                    $("#responseContainerTwo").append(stockInfo);
                });


            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                $("#responseContainerTwo").empty();
                let errorCard = `
                    <div class="card border-danger mb-3">
                        <div class="card-header bg-danger text-white">
                            <h5>⚠️ Error: API Limit Reached</h5>
                        </div>
                        <div class="card-body">
                            <p>You've reached the maximum number of API requests (25) allowed for now.</p>
                            <p>Please wait before trying again or consider upgrading your API plan if applicable.</p>
                            <p class="text-muted">Technical Info: ${error}</p>
                        </div>
                    </div>
                `;

                $("#responseContainerTwo").append(errorCard);
            }
        });
    }


    Recommendation = function (ticker) {
        $.ajax({
            url: "/api/stock/Recommendation",
            method: "GET",
            data: { ticker: ticker },
            dataType: "json",
            success: function (response) {
                if (!Array.isArray(response) || response.length === 0) {
                    $("#recommend").html("<p>No recommendation data available.</p>");
                    return;
                }

                // Sort by period descending (optional, in case API isn't ordered)
                response.sort((a, b) => b.period.localeCompare(a.period));

                let html = `
                <table border="1" cellpadding="5" cellspacing="0">
                <thead>
                    <tr>
                        <th>Period</th>
                        <th>Strong Buy</th>
                        <th>Buy</th>
                        <th>Hold</th>
                        <th>Sell</th>
                        <th>Strong Sell</th>
                    </tr>
                </thead>
                <tbody>
                `;

                response.forEach(item => {
                html += `
                    <tr>
                        <td>${item.period}</td>
                        <td>${item.strongBuy}</td>
                        <td>${item.buy}</td>
                        <td>${item.hold}</td>
                        <td>${item.sell}</td>
                        <td>${item.strongSell}</td>
                    </tr>
                `;
                });

                html += `
                    </tbody>
                    </table>
                `;

                $("#recommend").html(html);
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
            }
        });
    }

    Related = function (ticker) {
        $.ajax({
            url: "/api/stock/Related",
            method: "GET",
            data: { ticker: ticker },
            dataType: "json",
            success: function (response) {
                if (!Array.isArray(response) || response.length === 0) {
                    $("#peersContainer").hide().html("");
                    return;
                }

                const peers = response.filter(peer => peer.toUpperCase() !== ticker.toUpperCase());

                if (peers.length === 0) {
                    $("#peersContainer").hide().html("");
                    return;
                }

                let html = `
                    <div class="peers-card-header">Related Companies</div>
                    <ul class="peers-list">
                        ${peers.map(t => `<li  onclick="SubmitTrend('${t}')">${t}</li>`).join("")}
                    </ul>
                `;

                //let html = '';
                //peers.forEach(t => {
                //    html += `<li onclick="SubmitTrend('${t}')">${t})</li>`;
                //});

                $("#peersContainer").html(html).show();
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
            }
        });
    };


    Overview = function (ticker) {
        $.ajax({
            url: "/api/stock/Overview",
            method: "GET",
            data: { ticker: ticker },
            success: function (response) {
                if (!response || typeof response !== "string") {
                    $("#ai-overview").hide().html("");
                    return;
                }

                let html = `
                <div class="ai-overview-header">AI Overview</div>
                <div class="ai-overview-body">${response}</div>
            `;

                $("#ai-overview").html(html).show();
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                $("#ai-overview").hide().html("");
            }
        });
    };


    Outlook = function (ticker) {
        $.ajax({
            url: "/api/stock/Outlook",
            method: "GET",
            data: { ticker: ticker },
            success: function (response) {
                if (!response || typeof response !== "string") {
                    $("#ai-outlook").hide().html("");
                    return;
                }

                let html = `
                <div class="ai-overview-header">AI Outlook</div>
                <div class="ai-overview-body">${response}</div>
            `;

                $("#ai-outlook").html(html).show();
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                $("#ai-outlook").hide().html("");
            }
        });
    };



    SubmitTrend = function (ticker) {
        $("#tickerInput").val(ticker);
        Resubmission(ticker);
    }

    $.ajax({
        url: "/api/stock/Trending",
        method: "GET",
        success: function (data) {
            let html = "<div class='trending-header'>🔥 Trending Stocks</div><ul class='peers-list'>";
            data.forEach(t => {
                html += `<li onclick="SubmitTrend('${t.ticker}')">${t.ticker} (${t.dp.toFixed(2)}%)</li>`;
            });
            html += "</ul>";
            $("#trendingContainer").html(html).show();
        },
        error: function () {
            $("#trendingContainer").html("<p>Failed to load trending stocks.</p>");
        }
    });


    Status = function () {
        $.ajax({
            url: "/api/stock/MarketStatus",
            method: "GET",
            success: function (data) {
                let isOpen = data.isOpen;
                let openStatus = isOpen
                    ? `<span class="open">✅ Open</span>`
                    : `<span class="closed">❌ Closed</span>`;

                let marketRow = `
                    <div class="market-status-card d-flex justify-content-between align-items-center flex-wrap">
                        <div class="me-4"><span class="label">Exchange:</span> <span class="value">${data.exchange}</span></div>
                        <div class="me-4"><span class="label">Session:</span> <span class="value">${data.session}</span></div>
                        <div><span class="label">Status:</span> ${openStatus}</div>
                    </div>
                `;

                $("#marketStatus").html(marketRow);
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                $("#marketStatus").html(`
            <div class="alert alert-danger">
                Failed to load market status.
            </div>
        `);
            }
        });
    }

    Quote = function (symbol) {
        $.ajax({
            url: '/api/stock/Quote',
            method: 'GET',
            data: { symbol: symbol },
            success: function (data) {
                let quoteHtml = `
                    <div class="card border-info shadow-sm mb-4">
                        <div class="card-header bg-info text-white d-flex justify-content-between align-items-center">
                            <span class="h5 mb-0">📈 Quote Information</span>
                            <span class="badge badge-light text-white">${new Date().toLocaleTimeString()}</span>
                        </div>
                        <div class="card-body">
                            <div class="row mb-2">
                                <div class="col-sm-6">
                                    <h6>Current Price</h6>
                                    <p class="display-6">$${data.c.toFixed(2)}</p>
                                </div>
                                <div class="col-sm-6">
                                    <h6>Percent Change</h6>
                                    <p class="display-6">
                                        <span class="badge ${data.dp >= 0 ? 'badge-success' : 'badge-danger'}">
                                            ${data.dp >= 0 ? '+' : ''}${data.dp.toFixed(2)}%
                                        </span>
                                    </p>
                                </div>
                            </div>
                            <hr />
                            <div id="stockDetailsCard" class="card p-3 mb-3 shadow-sm text-sm">
                                <div class="row gy-2">
                                    <div class="col-sm-6"><strong>Change:</strong> $${data.d.toFixed(2)}</div>
                                    <div class="col-sm-6"><strong>Open:</strong> $${data.o.toFixed(2)}</div>
                                    <div class="col-sm-6"><strong>High:</strong> $${data.h.toFixed(2)}</div>
                                    <div class="col-sm-6"><strong>Low:</strong> $${data.l.toFixed(2)}</div>
                                    <div class="col-sm-6"><strong>Previous Close:</strong> $${data.pc.toFixed(2)}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                `;

                // Theme switch logic
                if (data.dp >= 0) {
                    $('#themeStylesheet').attr('href', '/css/GreenTheme.css');
                } else {
                    $('#themeStylesheet').attr('href', '/css/RedTheme.css');
                }

                $("#quoteContainer").html(quoteHtml);
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                $("#quoteContainer").html(`
                    <div class="alert alert-danger">
                        Failed to load market status.
                    </div>
                `);
            }
        });
    }


    //GetTrendingStocks();
    Status();

    $("#submitButton").click(function () {
        var ticker = $("#tickerInput").val();  // Get the ticker from the input

        if (!ticker) {
            alert("Please enter a ticker symbol");
            return;
        }

        Overview(ticker);
        Outlook(ticker);
        GetNews(ticker);
        Recommendation(ticker);
        Related(ticker);
        Quote(ticker);
    });

    function closeSuperchargePopup() {
        $("#superchargePopup").hide();
    }

    Supercharge = function (ticker) {
        $.ajax({
            url: "/api/stock/Supercharge",
            method: "GET",
            data: { symbol: ticker },
            success: function (response) {
                if (!response || typeof response !== "string") {
                    $("#superchargePopup").hide();
                    return;
                }

                // Optional: Format response if markdown-style text
                const formatted = response
                    .replace(/\*\*Buy Recommendation:\*\*/g, "<strong>Buy Recommendation:</strong>")
                    .replace(/\*\*Reasoning:\*\*/g, "<strong>Reasoning:</strong>")
                    .replace(/\*\*30-Day Price Prediction:\*\*/g, "<strong>30-Day Price Prediction:</strong>")
                    .replace(/\*\*Catalyst\(s\):\*\*/g, "<strong>Catalyst(s):</strong>")
                    .replace(/\n/g, "<br>");

                $("#superchargeContent").html(formatted);
                $("#superchargePopup").show();
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                $("#superchargePopup").hide();
            }
        });
    };

    $(".supercharge-close").click(function () {
        $("#superchargePopup").hide();
    });


    $("#superchargeButton").click(function () {
        var ticker = $("#tickerInput").val();  // Get the ticker from the input

        if (!ticker) {
            alert("Please enter a ticker symbol");
            return;
        }

        Supercharge(ticker);


    });

    Resubmission = function (ticker) {
        Overview(ticker);
        Outlook(ticker);
        GetNews(ticker);
        Recommendation(ticker);
        Related(ticker);
        Quote(ticker);
    }

});