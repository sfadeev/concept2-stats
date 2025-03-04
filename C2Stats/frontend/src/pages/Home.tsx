import { Card, DatePicker, Segmented, Select, Space, Typography } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useState } from 'react';
import WodCalendarData from '../components/WodCalendarData';
import WodDayBarData from '../components/WodDayBarData';
const { Text } = Typography;

export interface CountryDatum {
    code: string;
    name: string;
    value: number;
}

export default () => {

    const [date, setDate] = useState<Date | null>(new Date());
    const [type, setType] = useState<string | null>('rowerg');
    const [country, setCountry] = useState<string | null>();
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

        <Space>
            <DatePicker
                id='date'
                value={date ? dayjs(date) : null}
                onChange={(date, dateString) => {
                    setDate(date ? date.toDate() : null);
                }}
            />

            <Segmented<string>
                options={[
                    { value: 'rowerg', label: 'RowErg' },
                    { value: 'bikeerg', label: 'BikeErg' },
                    { value: 'skierg', label: 'SkiErg' },
                ]}
                onChange={(value) => setType(value)}
            />

            {countries ? (
                <Select
                    value={country}
                    allowClear={true}
                    showSearch={true}
                    filterOption={true}
                    optionFilterProp='label'
                    style={{ width: 220 }}
                    options={countries.map(x => { return { value: x.code, label: x.name, count: x.value }; })}
                    optionRender={(option) => (
                        <>
                            {option.data.label}
                            <Text style={{ float: 'right' }} type="secondary">{option.data.count}</Text>
                        </>
                    )}
                    onSelect={(value, country) => setCountry(country.value)}
                    onClear={() => setCountry(null)}
                />
            ) : null}
        </Space>

        {(date && type) ? (<>
            <Card size='small' style={{ marginTop: 20 }}>
                <div style={{ overflowX: 'auto', overflowY: 'hidden' }}>
                    <WodCalendarData
                        type={type}
                        year={date.getFullYear()}
                        country={country}
                        onClick={(date, event) => {
                            return setDate(date);
                        }}
                    />
                </div>

                <WodDayBarData
                    type={type}
                    date={date}
                    country={country}
                />
            </Card>

        </>) : null}

    </>);
};
