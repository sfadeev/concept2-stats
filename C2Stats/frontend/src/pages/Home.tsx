import { Card, DatePicker, Segmented, Select, Space } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useState } from 'react';
import WodCalendarData from '../components/WodCalendarData';
import WodDayBarData from '../components/WodDayBarData';

export interface Country {
    code: string;
    name: string;
}

export default () => {

    const [date, setDate] = useState<Date | null>(new Date());
    const [type, setType] = useState<string | null>('rowerg');
    const [country, setCountry] = useState<string | null>();
    const [countries, setCountries] = useState<Country[] | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetch(`/api/wod/countries`)
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
    }, []);

    return (<>

        <Space>
            <DatePicker id='date' value={date ? dayjs(date) : null} onChange={(date, dateString) => {
                setDate(date ? date.toDate() : null);
            }} />

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
                    allowClear={true}
                    showSearch={true}
                    filterOption={true}
                    optionFilterProp='label'
                    style={{ width: 180 }}
                    options={countries.map(x => { return { value: x.code, label: x.name }; })}
                    onSelect={(value) => setCountry(value)}
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
