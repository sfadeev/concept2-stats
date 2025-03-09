export function getDateString(date: Date): string {
    const offset = date.getTimezoneOffset();
    const dateWithOffset = new Date(date.getTime() - (offset * 60 * 1000));
    const result = dateWithOffset.toISOString().split('T')[0];
    return result;
};
