// src/utils/timeHelper.js
export function timeAgo(date, now = new Date()) {
    const then = new Date(date);
    const seconds = Math.floor((now - then) / 1000);
    const units = [
        { name: "year", value: 31536000 },
        { name: "month", value: 2592000 },
        { name: "week", value: 604800 },
        { name: "day", value: 86400 },
        { name: "hour", value: 3600 },
        { name: "minute", value: 60 }
    ];
    for (const unit of units) {
        const amount = Math.floor(seconds / unit.value);
        if (amount >= 1)
            return `${amount} ${unit.name}${amount > 1 ? 's' : ''} ago`;
    }
    return 'Just now';
}
