export function getDateString(date: Date): string {
    const offset = date.getTimezoneOffset();
    const dateWithOffset = new Date(date.getTime() - (offset * 60 * 1000));
    const result = dateWithOffset.toISOString().split('T')[0];
    return result;
};

export function isValidDate(dateString?: string): boolean {

    if (dateString) {
        let date = new Date(dateString);

        return !!date.getTime();
    }

    return false;
};

export function getValidDate(dateString?: string): Date {
    return isValidDate(dateString) ? new Date(dateString!) : new Date();
}
