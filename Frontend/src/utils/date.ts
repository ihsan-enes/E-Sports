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

export function getIstanbulDayKey(utcDateStr?: string | null): string {
  if (!utcDateStr) return 'unknown';

  try {
    const date = new Date(utcDateStr);
    // Format to YYYY-MM-DD in Europe/Istanbul
    const formatter = new Intl.DateTimeFormat('en-CA', {
      timeZone: 'Europe/Istanbul',
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    });
    return formatter.format(date);
  } catch {
    return 'unknown';
  }
}

export function formatTurkishDayHeading(utcDateStr?: string | null): string {
  if (!utcDateStr) return '-';

  try {
    const date = new Date(utcDateStr);
    const dayName = new Intl.DateTimeFormat('tr-TR', {
      timeZone: 'Europe/Istanbul',
      weekday: 'long'
    }).format(date);

    const dayAndMonth = new Intl.DateTimeFormat('tr-TR', {
      timeZone: 'Europe/Istanbul',
      day: 'numeric',
      month: 'long'
    }).format(date);

    // Capitalize first letter of day name (e.g. Salı)
    const capitalizedDayName = dayName.charAt(0).toUpperCase() + dayName.slice(1);

    return `${dayAndMonth} ${capitalizedDayName}`;
  } catch {
    return utcDateStr;
  }
}

export type DateRangePreset = 'this_week' | 'next_week' | 'this_month' | 'all';

export function getDateRangeUtc(preset: DateRangePreset): { from?: string; to?: string } {
  const now = new Date();

  // Europe/Istanbul UTC offset is fixed UTC+3
  // Convert current UTC time to Istanbul equivalent date representation
  const istanbulOffsetMs = 3 * 60 * 60 * 1000;
  const istanbulNow = new Date(now.getTime() + istanbulOffsetMs);

  if (preset === 'this_week') {
    // Current week starting from today to Sunday 23:59:59 (Istanbul)
    const dayOfWeek = istanbulNow.getUTCDay(); // 0 is Sunday, 1 is Monday
    const daysUntilSunday = (7 - (dayOfWeek === 0 ? 7 : dayOfWeek));

    const endOfSundayIstanbul = new Date(istanbulNow);
    endOfSundayIstanbul.setUTCDate(istanbulNow.getUTCDate() + daysUntilSunday);
    endOfSundayIstanbul.setUTCHours(23, 59, 59, 999);

    const endUtc = new Date(endOfSundayIstanbul.getTime() - istanbulOffsetMs);
    return {
      from: now.toISOString(),
      to: endUtc.toISOString()
    };
  }

  if (preset === 'next_week') {
    const dayOfWeek = istanbulNow.getUTCDay();
    const daysUntilNextMonday = dayOfWeek === 0 ? 1 : (8 - dayOfWeek);

    const nextMondayIstanbul = new Date(istanbulNow);
    nextMondayIstanbul.setUTCDate(istanbulNow.getUTCDate() + daysUntilNextMonday);
    nextMondayIstanbul.setUTCHours(0, 0, 0, 0);

    const nextSundayIstanbul = new Date(nextMondayIstanbul);
    nextSundayIstanbul.setUTCDate(nextMondayIstanbul.getUTCDate() + 6);
    nextSundayIstanbul.setUTCHours(23, 59, 59, 999);

    const startUtc = new Date(nextMondayIstanbul.getTime() - istanbulOffsetMs);
    const endUtc = new Date(nextSundayIstanbul.getTime() - istanbulOffsetMs);

    return {
      from: startUtc.toISOString(),
      to: endUtc.toISOString()
    };
  }

  if (preset === 'this_month') {
    const year = istanbulNow.getUTCFullYear();
    const month = istanbulNow.getUTCMonth();

    // Last day of current month in Istanbul
    const lastDayOfMonth = new Date(Date.UTC(year, month + 1, 0, 23, 59, 59, 999));
    const endUtc = new Date(lastDayOfMonth.getTime() - istanbulOffsetMs);

    return {
      from: now.toISOString(),
      to: endUtc.toISOString()
    };
  }

  return {
    from: now.toISOString()
  };
}
