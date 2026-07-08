/**
 * Format DateTimeOffset string to readable local time
 * Converts ISO 8601 format to local time and formats it nicely
 * @param dateTimeString ISO 8601 datetime string (e.g., "2026-07-08T00:05:04.071239+00:00")
 * @returns Formatted string (e.g., "08/07/2026 07:05:04")
 */
export function formatDateTime(dateTimeString: string | null | undefined): string {
  if (!dateTimeString) return '—';

  try {
    const date = new Date(dateTimeString);
    if (isNaN(date.getTime())) return '—';

    // Format to local time: DD/MM/YYYY HH:MM:SS
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');

    return `${day}/${month}/${year} ${hours}:${minutes}:${seconds}`;
  } catch (error) {
    return '—';
  }
}

/**
 * Format DateTimeOffset to relative time (e.g., "2 hours ago")
 * @param dateTimeString ISO 8601 datetime string
 * @returns Relative time string
 */
export function formatDateTimeRelative(dateTimeString: string | null | undefined): string {
  if (!dateTimeString) return '—';

  try {
    const date = new Date(dateTimeString);
    if (isNaN(date.getTime())) return '—';

    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSeconds = Math.floor(diffMs / 1000);
    const diffMinutes = Math.floor(diffSeconds / 60);
    const diffHours = Math.floor(diffMinutes / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffSeconds < 60) return 'Vừa xong';
    if (diffMinutes < 60) return `${diffMinutes} phút trước`;
    if (diffHours < 24) return `${diffHours} giờ trước`;
    if (diffDays < 7) return `${diffDays} ngày trước`;

    return formatDateTime(dateTimeString);
  } catch (error) {
    return '—';
  }
}

/**
 * Format DateTimeOffset to ISO format (YYYY-MM-DD)
 * @param dateTimeString ISO 8601 datetime string
 * @returns ISO date string
 */
export function formatDateOnly(dateTimeString: string | null | undefined): string {
  if (!dateTimeString) return '—';

  try {
    const date = new Date(dateTimeString);
    if (isNaN(date.getTime())) return '—';

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${day}/${month}/${year}`;
  } catch (error) {
    return '—';
  }
}
