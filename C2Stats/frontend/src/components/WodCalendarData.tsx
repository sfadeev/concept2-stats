import { Alert } from "antd";
import { useEffect, useState } from "react";
import WodCalendar, { WodCalendarProps } from "./WodCalendar";

export interface WodCalendarDataProps {
    type: string;
    year: number;
    country?: string | null;
    onClick?: ((date: Date, event: React.MouseEvent<SVGRectElement, MouseEvent>) => void) | undefined;
}

export default ({ type, year, country, onClick }: WodCalendarDataProps) => {

    const [loading, setLoading] = useState<boolean>(false);
    const [data, setData] = useState<WodCalendarProps | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {

        setLoading(true);

        fetch(`/api/wod/year?type=${type}&year=${year}&country=${country || ''}`)
            .then(response => {
                if (response.ok) return response.json();
                throw new Error(`${response.statusText} (${response.status})`);
            })
            .then(data => {
                setData(data);
                setLoading(false);
                return setError(null);
            })
            .catch(e => {
                setData(null);
                setLoading(false);
                return setError(e.message);
            });
    }, [type, year, country]);

    if (error) return <Alert message={error} type="error" />;

    return (<>
        {data ? (<WodCalendar
            from={data.from}
            to={data.to}
            data={data.data}
            onClick={(d, x) => { return (onClick ? onClick(d, x) : null); }}
        />) : null}
    </>);
};
