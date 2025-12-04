"use strict";

document.addEventListener("DOMContentLoaded", () => {
    // This should match the Blackjack game row in the DB
    const BLACKJACK_GAME_ID = 2; // Ensure DbSeeder gives Blackjack this Id
    const MIN_BET = 1;
    const MAX_BET = 500;

    // --- DOM elements ---
    const nameSpan = document.getElementById("bj-player-name");
    const emailSpan = document.getElementById("bj-player-email");
    const balanceSpan = document.getElementById("bj-player-balance");

    const dealerHandDiv = document.getElementById("bj-dealer-hand");
    const dealerTotalSpan = document.getElementById("bj-dealer-total");

    const playerHandDiv = document.getElementById("bj-player-hand");
    const playerTotalSpan = document.getElementById("bj-player-total");

    const outcomeText = document.getElementById("bj-outcome-text");
    const errorText = document.getElementById("bj-error-text");

    const playBtn = document.getElementById("bj-play-btn");
    const playLabel = document.getElementById("bj-play-label");
    const refreshBalanceBtn = document.getElementById("bj-refresh-balance");

    const betInput = document.getElementById("bj-bet-input");

    // --- User from localStorage ---
    const userJson = localStorage.getItem("user");
    if (!userJson) {
        alert("You must be logged in to play Blackjack.");
        window.location.href = "/Account/Login";
        return;
    }

    let user = JSON.parse(userJson);
    const userId = user.id;
    let currentBet = 10;

    // --- Helpers ---

    /// <summary>
    /// Updates the text on the main “Deal” button to show the
    /// current bet amount so the user knows what they’re risking.
    /// </summary>
    function updatePlayLabel() {
        if (!playLabel) return;
        playLabel.textContent = `Deal Hand for $${currentBet.toFixed(2)}`;
    }



    /// Normalizes the bet input:
    /// falls back to MIN_BET if invalid
    /// clamps between MIN_BET and MAX_BET
    /// also caps the bet at the player’s current balance.
    /// </summary>
    function setBetFromInput() {
        if (!betInput) return;

        let raw = Number(betInput.value);
        if (Number.isNaN(raw) || raw <= 0) {
            raw = MIN_BET;
        }

        raw = Math.max(MIN_BET, Math.min(MAX_BET, raw));

        const balance = Number(balanceSpan.textContent) || 0;
        if (raw > balance && balance > 0) {
            raw = balance;
        }

        currentBet = raw;
        betInput.value = String(raw);
        updatePlayLabel();
    }




    /// <summary>
    /// Pulls fresh user data from the API (in case balance changed
    /// elsewhere) and syncs both the UI and localStorage copy.
    /// </summary>
    async function refreshUserFromApi() {
        try {
            const res = await fetch(`/api/user/${userId}`);
            if (!res.ok) return;

            const data = await res.json();
            user = data;
            nameSpan.textContent = `${data.firstName} ${data.lastName}`;
            emailSpan.textContent = data.email;
            balanceSpan.textContent = (data.demoBalance ?? 0).toFixed(2);

            localStorage.setItem("user", JSON.stringify(data));
            setBetFromInput();
        } catch (err) {
            console.error("Error refreshing user on Blackjack page:", err);
        }
    }


    /// <summary>
    /// Renders a Blackjack hand as a simple string 
    /// and sets the total span. If there are no cards, shows a dash.
    /// </summary>
    function renderHand(cards, container, totalSpan) {
        if (!container || !totalSpan) return;

        if (!cards || cards.length === 0) {
            container.textContent = "—";
            totalSpan.textContent = "0";
            return;
        }

        container.textContent = cards.join(" ");
        // totalSpan is set from API 
    }

    // --- Initial UI from localStorage ---
    nameSpan.textContent = `${user.firstName} ${user.lastName}`;
    emailSpan.textContent = user.email;
    balanceSpan.textContent = (user.demoBalance ?? 0).toFixed(2);

    if (betInput) {
        betInput.value = "10";
        updatePlayLabel();
    }

    // Pull a fresh copy from the backend on load

    refreshUserFromApi();

    // --- Handlers ---

    if (betInput) {
        betInput.addEventListener("input", () => {
            setBetFromInput();
            if (errorText) errorText.textContent = "";
        });
    }

    if (refreshBalanceBtn) {
        refreshBalanceBtn.addEventListener("click", () => {
            refreshUserFromApi();
            if (errorText) errorText.textContent = "";
        });
    }



    /// <summary>
    /// Validates the bet, sends a /api/blackjack/play request,
    /// then updates the balance, player/dealer hands, and outcome
    /// message based on the server-side game result.
    /// </summary>
    if (playBtn) {
        playBtn.addEventListener("click", async () => {
            if (errorText) errorText.textContent = "";

            setBetFromInput();
            const balance = Number(balanceSpan.textContent) || 0;

            if (currentBet < MIN_BET || currentBet > MAX_BET) {
                if (errorText) {
                    errorText.textContent =
                        `Bet must be between $${MIN_BET} and $${MAX_BET}.`;
                }
                return;
            }

            if (currentBet > balance) {
                if (errorText) {
                    errorText.textContent = "Insufficient balance for this bet.";
                }
                return;
            }

            if (outcomeText) {
                outcomeText.textContent = "Dealing...";
            }

            playBtn.disabled = true;

            try {
                const body = new URLSearchParams({
                    UserId: userId,
                    GameId: BLACKJACK_GAME_ID,
                    BetAmount: currentBet
                });

                const res = await fetch("/api/blackjack/play", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: body.toString()
                });

                if (!res.ok) {
                    const text = await res.text();
                    console.error("Blackjack play error:", text);

                    if (errorText) {
                        errorText.textContent =
                            res.status === 400 && text
                                ? text
                                : "Hand failed. Check your bet and try again.";
                    }

                    if (outcomeText) {
                        outcomeText.textContent = "Error dealing hand.";
                    }
                    return;
                }

                const data = await res.json();

                // Balance + localStorage update
                const newBal = data.newBalance ?? 0;
                balanceSpan.textContent = newBal.toFixed(2);
                user.demoBalance = newBal;
                localStorage.setItem("user", JSON.stringify(user));

                setBetFromInput();

                // Render hands
                renderHand(data.playerHand, playerHandDiv, playerTotalSpan);
                renderHand(data.dealerHand, dealerHandDiv, dealerTotalSpan);

                if (typeof data.playerTotal === "number") {
                    playerTotalSpan.textContent = String(data.playerTotal);
                }
                if (typeof data.dealerTotal === "number") {
                    dealerTotalSpan.textContent = String(data.dealerTotal);
                }

                if (outcomeText) {
                    if (typeof data.message === "string" && data.message.length > 0) {
                        outcomeText.textContent = data.message;
                    } else {
                        outcomeText.textContent = data.outcome || "Hand complete.";
                    }
                }
            } catch (err) {
                console.error("Error during Blackjack hand:", err);
                if (errorText) {
                    errorText.textContent = "Unexpected error during hand.";
                }
                if (outcomeText) {
                    outcomeText.textContent = "Error dealing hand.";
                }
            } finally {
                playBtn.disabled = false;
            }
        });
    }
});