import { useEffect, useState } from "react";
import WodDayBar, { WodDayBarProps } from "./WodDayBar";

export interface WodDayBarDataProps {
    type: string;
    date?: Date | null;
    country?: string | null;
}

export default ({ type, date, country }: WodDayBarDataProps) => {

    const [data, setData] = useState<WodDayBarProps | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {

        const offset = date?.getTimezoneOffset();
        const dateWithOffset = date && offset ? new Date(date.getTime() - (offset * 60 * 1000)) : null;
        const dateStr = dateWithOffset?.toISOString().split('T')[0] || '';

        fetch(`/api/wod/day?type=${type}&date=${dateStr}&country=${country || ''}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch data');
                }
                return response.json();
            })
            .then(data => {
                setData(data);
            })
            .catch(err => setError('Error fetching data'));
    }, [type, date, country]);

    return (
        <>
            {error ? (
                <p style={{ color: 'red' }}>{error}</p>
            ) : (
                data ? (
                    <WodDayBar data={data.data} />
                ) : (
                    <div>Loading...</div>
                )
            )}
        </>
    );
};
