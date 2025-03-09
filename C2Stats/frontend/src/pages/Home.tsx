import { Card, DatePicker, Segmented, Space } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import CountrySelector from '../components/CountrySelector';
import ProfileSelector from '../components/ProfileSelector';
import WodCalendarData from '../components/WodCalendarData';
import WodDayBarData from '../components/WodDayBarData';
import { getDateString } from '../utils';

const DEFAULT_TYPE = 'rowerg';

type HomeParams = {
    qdate: string;
    qtype: string;
};

export default () => {

    const navigate = useNavigate();

    const go = (date: Date, type: string = DEFAULT_TYPE) => {
        navigate(`/${getDateString(date)}/${type}`);
    };

    let { qdate, qtype } = useParams<HomeParams>();

    useEffect(() => {
        if (!qdate || !qtype) {
            go(qdate ? new Date(qdate) : new Date(), qtype);
        }

        setDate(new Date(qdate!));
        setType(qtype);

    }, [qdate, qtype]);

    const [date, setDate] = useState<Date | undefined>();
    const [type, setType] = useState<string | undefined>();
    const [country, setCountry] = useState<string | undefined>();

    return (<>
        <Space>
            <DatePicker
                id='date'
                allowClear={false}
                value={date ? dayjs(date) : null}
                onChange={(date, _dateString) => go(date.toDate(), type)}
            />

            <Segmented<string>
                value={type}
                options={[
                    { value: 'rowerg', label: 'RowErg' },
                    { value: 'bikeerg', label: 'BikeErg' },
                    { value: 'skierg', label: 'SkiErg' },
                ]}
                onChange={(value) => go(date!, value)}
            />

            <CountrySelector
                type={type}
                date={date}
                onChange={(value) => setCountry(value)}
            />

            <ProfileSelector />
        </Space>

        {(date && type) ? (<>
            <Card size='small' style={{ marginTop: 20 }}>
                <div style={{ overflowX: 'auto', overflowY: 'hidden' }}>
                    <WodCalendarData
                        type={type}
                        year={date.getFullYear()}
                        country={country}
                        onClick={(date, event) => {
                            return go(date, type);
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
