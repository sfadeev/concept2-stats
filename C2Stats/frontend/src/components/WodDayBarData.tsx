import { useEffect, useState } from "react";
import WodDayBar, { WodDayBarProps } from "./WodDayBar";

export interface WodDayBarDataProps {
    wodType: string;
}

export default ({ wodType }: WodDayBarDataProps) => {

    const [data, setData] = useState<WodDayBarProps | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetch(`/api/wod/day?year=${12345}&wodType=${wodType}`)
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
                    <WodDayBar data={data.data} />
                ) : (
                    <div>Loading...</div>
                )
            )}
        </>
    );
};
