"use strict";

document.addEventListener("DOMContentLoaded", () => {
    // --- Get current user from localStorage ---
    const userJson = localStorage.getItem("user");
    if (!userJson) {
        alert("You must be logged in to view the lobby.");
        window.location.href = "/Account/Login";
        return;
    }

    const user = JSON.parse(userJson);
    const userId = user.id;

    // --- DOM refs ---
    const welcomeText = document.getElementById("welcome-text");
    const balanceSpan = document.getElementById("lobby-balance");
    const emailBadge = document.getElementById("lobby-email");
    const logoutBtn = document.getElementById("logout-btn");

    const favoriteButtons = document.querySelectorAll(".favorite-game-btn");

    // --- Fill basic info  ---
    const fullName = `${user.firstName ?? ""} ${user.lastName ?? ""}`.trim();
    if (welcomeText) {
        welcomeText.textContent = fullName
            ? `Welcome back, ${fullName}.`
            : "Welcome back.";
    }

    if (balanceSpan) {
        balanceSpan.textContent = (user.demoBalance ?? 0).toFixed(2);
    }

    if (emailBadge) {
        emailBadge.textContent = user.email ?? "";
    }

    // Logout: clear localStorage and bounce to login
    if (logoutBtn) {
        logoutBtn.addEventListener("click", () => {
            localStorage.removeItem("user");
            window.location.href = "/Account/Login";
        });
    }

    // ---------- Favorites logic ----------
    // Tracks which game IDs are favorited for this user in-memory,
    // so the UI can stay in sync without reloading from the API every click.
    const favoriteGameIds = new Set();

    // Updates visual state of a star button (filled or hollow).
    function setButtonFavoriteState(btn, isFavorite) {
        if (!btn) return;
        const starSpan = btn.querySelector(".favorite-star");

        if (isFavorite) {
            btn.classList.remove("btn-outline-warning");
            btn.classList.add("btn-warning");
            if (starSpan) starSpan.textContent = "★";
            btn.title = "Click to remove from favorites";
        } else {
            btn.classList.remove("btn-warning");
            btn.classList.add("btn-outline-warning");
            if (starSpan) starSpan.textContent = "☆";
            btn.title = "Click to mark as favorite";
        }
    }



    // Loads all favorite games for the current user from the API
    // and updates the local set + star buttons in the lobby.
    async function loadFavoritesForUser() {
        try {
            const res = await fetch(`/api/favorites/user/${userId}`);
            if (!res.ok) {
                console.warn("Failed to load favorites for user", userId);
                return;
            }

            const games = await res.json();
            favoriteGameIds.clear();

            if (Array.isArray(games)) {
                for (const g of games) {
                    if (typeof g.id === "number") {
                        favoriteGameIds.add(g.id);
                    }
                }
            }

            // Apply state to all star buttons
            favoriteButtons.forEach(btn => {
                const gameIdAttr = btn.getAttribute("data-game-id");
                const gameId = gameIdAttr ? parseInt(gameIdAttr, 10) : NaN;
                if (!Number.isNaN(gameId)) {
                    const isFav = favoriteGameIds.has(gameId);
                    setButtonFavoriteState(btn, isFav);
                }
            });
        } catch (err) {
            console.error("Error loading favorites:", err);
        }
    }



    // Adds or removes a single game from the user’s favorites,
    // then flips the star button state if the API call succeeds.
    async function toggleFavorite(gameId, btn) {
        const isCurrentlyFavorite = favoriteGameIds.has(gameId);

        try {
            if (isCurrentlyFavorite) {
                // Remove favorite
                const res = await fetch(`/api/favorites/user/${userId}/game/${gameId}`, {
                    method: "DELETE"
                });

                if (!res.ok) {
                    console.error("Failed to remove favorite for game", gameId);
                    return;
                }

                favoriteGameIds.delete(gameId);
                setButtonFavoriteState(btn, false);
            } else {
                // Add favorite
                const res = await fetch(`/api/favorites/user/${userId}/game/${gameId}`, {
                    method: "POST"
                });

                if (!res.ok) {
                    console.error("Failed to add favorite for game", gameId);
                    return;
                }

                favoriteGameIds.add(gameId);
                setButtonFavoriteState(btn, true);
            }
        } catch (err) {
            console.error("Error toggling favorite:", err);
        }
    }

    // Wire up click handlers for all favorite buttons
    favoriteButtons.forEach(btn => {
        const gameIdAttr = btn.getAttribute("data-game-id");
        const gameId = gameIdAttr ? parseInt(gameIdAttr, 10) : NaN;
        if (Number.isNaN(gameId)) return;

        btn.addEventListener("click", () => {
            toggleFavorite(gameId, btn);
        });
    });

    // Initial load of favorites from API
    loadFavoritesForUser();
});