import { Select, Typography } from "antd";
import { useEffect, useState } from "react";
const { Text } = Typography;

export interface CountryDatum {
    code: string;
    name: string;
    value: number;
}

export interface CountrySelectorProps {
    type: string;
    date: Date;
    country?: string;
    onChange: (value?: string) => void;
}

export default ({ type, date, country, onChange }: CountrySelectorProps) => {

    const [countries, setCountries] = useState<CountryDatum[] | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const year = date?.getFullYear();

        if (year) {
            fetch(`/api/wod/countries?type=${type}&year=${year}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Failed to fetch data');
                    }
                    return response.json();
                })
                .then(data => {
                    setCountries(data);
                })
                .catch(err => setError('Error fetching data'));
        }
        else {
            setCountries(null);
        }
    }, [type, date?.getFullYear()]);

    return (<>
        {countries ? (
            <Select
                value={country}
                allowClear={true}
                showSearch={true}
                filterOption={true}
                optionFilterProp='label'
                style={{ width: 200 }}
                options={countries.map(x => { return { value: x.code, label: x.name, count: x.value }; })}
                optionRender={(option) => (
                    <>
                        {option.data.label}
                        <Text style={{ float: 'right' }} type="secondary">{option.data.count}</Text>
                    </>
                )}
                onSelect={(_, country) => onChange(country.value)}
                onClear={() => onChange(undefined)}
            />
        ) : null}
    </>);
};
