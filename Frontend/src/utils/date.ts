export function formatMatchTime(utcDateStr?: string | null): string {
  if (!utcDateStr) return '-';

  try {
    const date = new Date(utcDateStr);
    return new Intl.DateTimeFormat('tr-TR', {
      timeZone: 'Europe/Istanbul',
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  } catch {
    return utcDateStr;
  }
}

export function formatTimeOnly(utcDateStr?: string | null): string {
  if (!utcDateStr) return '-';

  try {
    const date = new Date(utcDateStr);
    return new Intl.DateTimeFormat('tr-TR', {
      timeZone: 'Europe/Istanbul',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  } catch {
    return utcDateStr;
  }
}
