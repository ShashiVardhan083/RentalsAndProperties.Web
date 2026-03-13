// Reusable chat utilities — used by the Conversation view
// The main SignalR logic is inline in Conversation.cshtml for page-specific config.
// This file provides shared helpers.

window.ChatUtils = {
    escapeHtml(text) {
        return text
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    },

    formatTime(isoString) {
        return new Date(isoString).toLocaleTimeString([], {
            hour: '2-digit', minute: '2-digit'
        });
    },

    getUnreadCount() {
        return parseInt(
            document.querySelector('.chat-unread-badge')?.textContent || '0', 10
        );
    },

    updateUnreadBadge(count) {
        const badge = document.querySelector('.chat-unread-badge');
        if (!badge) return;
        badge.textContent = count > 0 ? count : '';
        badge.style.display = count > 0 ? 'flex' : 'none';
    }
};
async function sendMessage() {
    const text = document.getElementById("messageInput").value;

    await fetch("/api/chat/send", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            propertyId: propertyId,
            receiverId: otherUserId,
            messageText: text
        })
    });

    location.reload(); // temporary refresh
}