// booking-notifications.js
// SignalR real-time notification client for booking status updates

let bookingConnection = null;

// Initialize SignalR connection
function initializeBookingNotifications() {
    if (bookingConnection) {
        console.log('Booking notifications already initialized');
        return;
    }

    // Create SignalR connection
    bookingConnection = new signalR.HubConnectionBuilder()
        .withUrl("/bookingHub")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Handle booking status updates
    bookingConnection.on("BookingStatusUpdated", function (data) {
        console.log('Booking status updated:', data);
        showBookingNotification(data);
        
        // Refresh bookings list if on MyBookings page
        if (window.location.pathname.includes('/Bookings/MyBookings')) {
            refreshBookingsList();
        }
    });

    // Handle booking completion
    bookingConnection.on("BookingCompleted", function (data) {
        console.log('Booking completed:', data);
        showBookingCompletionNotification(data);
        
        // Refresh bookings list if on MyBookings page
        if (window.location.pathname.includes('/Bookings/MyBookings')) {
            refreshBookingsList();
        }
    });

    // Handle reconnecting
    bookingConnection.onreconnecting(error => {
        console.warn('SignalR reconnecting:', error);
        showReconnectingMessage();
    });

    // Handle reconnected
    bookingConnection.onreconnected(connectionId => {
        console.log('SignalR reconnected:', connectionId);
        hideReconnectingMessage();
    });

    // Handle disconnected
    bookingConnection.onclose(error => {
        console.error('SignalR disconnected:', error);
        showDisconnectedMessage();
        
        // Try to reconnect after 5 seconds
        setTimeout(() => {
            startBookingConnection();
        }, 5000);
    });

    // Start connection
    startBookingConnection();
}

// Start SignalR connection
function startBookingConnection() {
    if (!bookingConnection) {
        console.error('Booking connection not initialized');
        return;
    }

    bookingConnection.start()
        .then(() => {
            console.log('? SignalR connected - Booking notifications active');
        })
        .catch(error => {
            console.error('SignalR connection failed:', error);
            // Retry after 5 seconds
            setTimeout(() => {
                startBookingConnection();
            }, 5000);
        });
}

// Show booking notification with toast/modal
function showBookingNotification(data) {
    const { bookingId, status, message, timestamp } = data;
    
    // Determine notification type
    const isApproved = status === 'Approved';
    const isRejected = status === 'Rejected';
    
    const notificationType = isApproved ? 'success' : (isRejected ? 'error' : 'info');
    const icon = isApproved ? '?' : (isRejected ? '?' : '??');
    const title = isApproved ? 'Booking Approved!' : (isRejected ? 'Booking Rejected' : 'Booking Update');
    
    // Show toast notification
    showToast(title, message, notificationType, icon);
    
    // Update badge count
    updateNotificationBadge();
    
    // Play notification sound
    playNotificationSound();
    
    // Show browser notification if supported and permitted
    showBrowserNotification(title, message, notificationType);
}

// Show booking completion notification
function showBookingCompletionNotification(data) {
    const { bookingId, roomName, endTime, message, timestamp } = data;
    
    showToast(
        '?? Booking Completed',
        message,
        'info',
        '?'
    );
    
    // Update badge count
    updateNotificationBadge();
    
    // Play completion sound
    playCompletionSound();
}

// Show toast notification
function showToast(title, message, type = 'info', icon = '??') {
    // Check if toast container exists, create if not
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }
    
    // Create toast element
    const toastId = `toast-${Date.now()}`;
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white bg-${type === 'success' ? 'success' : (type === 'error' ? 'danger' : 'primary')} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${icon} ${title}</strong><br>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;
    
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    // Initialize and show toast
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: 8000
    });
    toast.show();
    
    // Remove toast element after hide
    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}

// Show browser notification
function showBrowserNotification(title, message, type) {
    // Check if browser supports notifications
    if (!("Notification" in window)) {
        return;
    }
    
    // Check permission
    if (Notification.permission === "granted") {
        createNotification(title, message, type);
    } else if (Notification.permission !== "denied") {
        Notification.requestPermission().then(permission => {
            if (permission === "granted") {
                createNotification(title, message, type);
            }
        });
    }
}

// Create browser notification
function createNotification(title, message, type) {
    const icon = type === 'success' ? '?' : (type === 'error' ? '?' : '??');
    
    const notification = new Notification(title, {
        body: message,
        icon: '/favicon.ico',
        badge: '/favicon.ico',
        tag: 'booking-notification',
        requireInteraction: false
    });
    
    notification.onclick = function() {
        window.focus();
        notification.close();
        
        // Navigate to My Bookings page
        if (!window.location.pathname.includes('/Bookings/MyBookings')) {
            window.location.href = '/Bookings/MyBookings';
        }
    };
    
    // Auto close after 10 seconds
    setTimeout(() => {
        notification.close();
    }, 10000);
}

// Update notification badge
function updateNotificationBadge() {
    const badge = document.querySelector('.notification-badge');
    if (badge) {
        const currentCount = parseInt(badge.textContent) || 0;
        badge.textContent = currentCount + 1;
        badge.style.display = 'inline-block';
    }
}

// Refresh bookings list
function refreshBookingsList() {
    // Find and click refresh button if exists
    const refreshBtn = document.getElementById('refresh-bookings-btn');
    if (refreshBtn) {
        refreshBtn.click();
        return;
    }
    
    // Otherwise reload page
    console.log('Reloading page to show updated bookings...');
    setTimeout(() => {
        window.location.reload();
    }, 2000);
}

// Play notification sound
function playNotificationSound() {
    try {
        const audio = new Audio('/sounds/notification.mp3');
        audio.volume = 0.5;
        audio.play().catch(e => console.log('Could not play sound:', e));
    } catch (e) {
        console.log('Notification sound not available');
    }
}

// Play completion sound
function playCompletionSound() {
    try {
        const audio = new Audio('/sounds/completion.mp3');
        audio.volume = 0.3;
        audio.play().catch(e => console.log('Could not play sound:', e));
    } catch (e) {
        console.log('Completion sound not available');
    }
}

// Show reconnecting message
function showReconnectingMessage() {
    showToast('Connection Lost', 'Reconnecting to notification service...', 'warning', '??');
}

// Hide reconnecting message
function hideReconnectingMessage() {
    showToast('Connected', 'Notification service reconnected!', 'success', '?');
}

// Show disconnected message
function showDisconnectedMessage() {
    showToast('Disconnected', 'Notification service disconnected. Will retry...', 'error', '?');
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    // Check if user is logged in (has userId)
    const userIdElement = document.querySelector('[data-user-id]');
    if (userIdElement) {
        console.log('Initializing booking notifications for user');
        initializeBookingNotifications();
    } else {
        console.log('User not logged in, skipping notification initialization');
    }
});

// Clean up on page unload
window.addEventListener('beforeunload', function() {
    if (bookingConnection) {
        bookingConnection.stop();
    }
});
