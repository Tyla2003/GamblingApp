"use strict";

document.addEventListener("DOMContentLoaded", () => {
    // These should match the Slots Game row in the DB
    const SLOTS_GAME_ID = 1;   // Slots game Id from DbSeeder
    const MIN_BET = 1;
    const MAX_BET = 500;

    // --- DOM elements ---
    const nameSpan = document.getElementById("slots-player-name");
    const emailSpan = document.getElementById("slots-player-email");
    const balanceSpan = document.getElementById("slots-player-balance");

    const reel1Img = document.getElementById("reel-1-img");
    const reel2Img = document.getElementById("reel-2-img");
    const reel3Img = document.getElementById("reel-3-img");

    const resultText = document.getElementById("slots-result-text");
    const errorText = document.getElementById("slots-error-text");

    const spinBtn = document.getElementById("slots-spin-btn");
    const spinLabel = document.getElementById("slots-spin-label");
    const refreshBalanceBtn = document.getElementById("slots-refresh-balance");

    const betInput = document.getElementById("slots-bet-input");
    const betHint = document.getElementById("slots-bet-hint");

    // --- User from localStorage ---
    const userJson = localStorage.getItem("user");
    if (!userJson) {
        alert("You must be logged in to play slots.");
        window.location.href = "/Account/Login";
        return;
    }

    let user = JSON.parse(userJson);
    const userId = user.id;

    // Track current bet amount
    let currentBet = 2;

    // Updates the button label so it always reflects the current bet size.
    function updateSpinLabel() {
        if (!spinLabel) return;
        spinLabel.textContent = `Spin for $${currentBet.toFixed(2)}`;
    }

    function setBetFromInput() {
        if (!betInput) return;

        let raw = Number(betInput.value);
        if (Number.isNaN(raw) || raw <= 0) {
            raw = MIN_BET;
        }

        // Clamp between MIN_BET / MAX_BET
        raw = Math.max(MIN_BET, Math.min(MAX_BET, raw));

        // Also clamp to current balance just to avoid obvious failures
        const balance = Number(balanceSpan.textContent) || 0;
        if (raw > balance && balance > 0) {
            raw = balance;
        }

        currentBet = raw;
        betInput.value = String(raw);
        updateSpinLabel();
    }

    // Maps the symbol string returned from the API into the correct slot image path.
    function symbolToImage(symbolString) {
        const s = (symbolString || "").toLowerCase();

        if (s === "jackpot") return "/images/slots/lucky7.png";
        if (s === "large")   return "/images/slots/diamond.png";
        if (s === "medium")  return "/images/slots/cherry.png";
        // default / small
        return "/images/slots/lemon.png";
    }

    // Pulls fresh user info (balance, email, etc.) from the API
    // and syncs it back into localStorage + the UI.
    async function refreshUserFromApi() {
        try {
            const res = await fetch(`/api/user/${userId}`);
            if (!res.ok) return;

            const data = await res.json();
            user = data; // keep local copy
            nameSpan.textContent = `${data.firstName} ${data.lastName}`;
            emailSpan.textContent = data.email;
            balanceSpan.textContent = (data.demoBalance ?? 0).toFixed(2);

            localStorage.setItem("user", JSON.stringify(data));

            // After balance updates, ensure bet is still valid
            setBetFromInput();
        } catch (err) {
            console.error("Error refreshing user on slots page:", err);
        }
    }

    // --- Initial population ---
    nameSpan.textContent = `${user.firstName} ${user.lastName}`;
    emailSpan.textContent = user.email;
    balanceSpan.textContent = (user.demoBalance ?? 0).toFixed(2);

    // Set default bet UI
    if (betInput) {
        betInput.value = "2";
        updateSpinLabel();
    }

    // Pull a fresh copy from the backend on load
    refreshUserFromApi();

    // --- Bet input change handler ---
    if (betInput) {
        betInput.addEventListener("input", () => {
            setBetFromInput();
            errorText.textContent = "";
        });
    }

    // --- Balance refresh button ---
    if (refreshBalanceBtn) {
        refreshBalanceBtn.addEventListener("click", () => {
            refreshUserFromApi();
            errorText.textContent = "";
        });
    }

    // --- Spin handler ---
    if (spinBtn) {
        spinBtn.addEventListener("click", async () => {
            errorText.textContent = "";

            // Ensure bet value is synced
            setBetFromInput();

            const balance = Number(balanceSpan.textContent) || 0;

            if (currentBet < MIN_BET || currentBet > MAX_BET) {
                errorText.textContent =
                    `Bet must be between $${MIN_BET} and $${MAX_BET}.`;
                return;
            }

            if (currentBet > balance) {
                errorText.textContent = "Insufficient balance for this bet.";
                return;
            }

            resultText.textContent = "Spinning...";
            spinBtn.disabled = true;

            try {
                const body = new URLSearchParams({
                    UserId: userId,
                    GameId: SLOTS_GAME_ID,
                    BetAmount: currentBet
                });

                const res = await fetch("/api/slots/spin", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: body.toString()
                });

                if (!res.ok) {
                    const text = await res.text();
                    console.error("Slots spin error:", text);

                    if (res.status === 400 && text) {
                        errorText.textContent = text;
                    } else {
                        errorText.textContent =
                            "Spin failed. Check your bet and try again.";
                    }

                    resultText.textContent = "Spin error.";
                    return;
                }

                const data = await res.json();

                // Update balance UI and local storage
                const newBal = data.newBalance ?? 0;
                balanceSpan.textContent = newBal.toFixed(2);
                user.demoBalance = newBal;
                localStorage.setItem("user", JSON.stringify(user));

                // Adjust bet if it now exceeds balance
                setBetFromInput();

                // Parse Spin Results from API 
                const display = data.displayResult || "";
                const parts = display.split("|").map(p => p.trim());

                const sym1 = parts[0] || "Small";
                const sym2 = parts[1] || "Small";
                const sym3 = parts[2] || "Small";

                reel1Img.src = symbolToImage(sym1);
                reel1Img.alt = sym1;

                reel2Img.src = symbolToImage(sym2);
                reel2Img.alt = sym2;

                reel3Img.src = symbolToImage(sym3);
                reel3Img.alt = sym3;

                if (data.bet && typeof data.bet.payoutAmount === "number") {
                    const payout = data.bet.payoutAmount;
                    if (payout > 0) {
                        resultText.textContent =
                            `${display} → You won $${payout.toFixed(2)}!`;
                    } else {
                        resultText.textContent =
                            `${display} → No win this time.`;
                    }
                } else {
                    resultText.textContent = display || "Spin completed.";
                }
            } catch (err) {
                console.error("Error during slots spin:", err);
                errorText.textContent = "Unexpected error during spin.";
                resultText.textContent = "Spin error.";
            } finally {
                spinBtn.disabled = false;
            }
        });
    }
});