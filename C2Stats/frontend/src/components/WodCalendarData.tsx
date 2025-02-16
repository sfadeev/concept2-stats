import { useEffect, useState } from "react";
import WodCalendar, { WodCalendarProps } from "./WodCalendar";

export interface WodCalendarDataProps {
    wodType: string;
    year: number;
}

export default ({ wodType, year }: WodCalendarDataProps) => {

    const [yearData, setYearData] = useState<WodCalendarProps | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetch(`/api/WodStats/Year?year=${year}&wodType=${wodType}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch data');
                }
                return response.json();
            })
            .then(data => {
                setYearData(data);
            })
            .catch(err => setError('Error fetching data'));
    }, []);

    return (
        <>
            {error ? (
                <p style={{ color: 'red' }}>{error}</p>
            ) : (
                yearData ? (
                    <WodCalendar from={yearData.from} to={yearData.to} data={yearData.data} />
                ) : (
                    <div>Loading...</div>
                )
            )}
        </>
    );
};
