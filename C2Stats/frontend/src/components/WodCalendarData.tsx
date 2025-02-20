import { useEffect, useState } from "react";
import WodCalendar, { WodCalendarProps } from "./WodCalendar";

export interface WodCalendarDataProps {
    year: number;
    wodType: string;
}

export default ({ year, wodType }: WodCalendarDataProps) => {

    const [data, setData] = useState<WodCalendarProps | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetch(`/api/wod/year?year=${year}&wodType=${wodType}`)
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
    }, []);

    return (
        <>
            {error ? (
                <p style={{ color: 'red' }}>{error}</p>
            ) : (
                data ? (
                    <WodCalendar from={data.from} to={data.to} data={data.data} />
                ) : (
                    <div>Loading...</div>
                )
            )}
        </>
    );
};
