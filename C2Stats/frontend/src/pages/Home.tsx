import { DoubleLeftOutlined, DoubleRightOutlined } from '@ant-design/icons';
import { Button, Card, DatePicker, Segmented, Space } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import CountrySelector from '../components/CountrySelector';
import ProfileSelector from '../components/ProfileSelector';
import WodCalendarData from '../components/WodCalendarData';
import WodDayBarData from '../components/WodDayBarData';
import WodItemTable from '../components/WodItemTable';
import { getDateString, getValidDate, isValidDate } from '../services/dateService';

const DEFAULT_TYPE = 'rowerg';

type HomeParams = {
    qdate: string;
    qtype: string;
};

const HomePage = () => {

    const navigate = useNavigate();

    const go = (date: Date, type: string = DEFAULT_TYPE) => {
        navigate(`/${getDateString(date)}/${type}`);
    };

    const goToDate = (num: number) => {
        const d = date ?? new Date();
        d.setDate(d.getDate() + num);
        go(d, type);
    };

    let { qdate, qtype } = useParams<HomeParams>();

    useEffect(() => {
        if (!qdate || !isValidDate(qdate) || !qtype) {
            go(getValidDate(qdate), qtype);
        }

        setDate(new Date(qdate!));
        setType(qtype);

    }, [qdate, qtype]);

    const [date, setDate] = useState<Date | undefined>();
    const [type, setType] = useState<string | undefined>();
    const [country, setCountry] = useState<string | undefined>();

    return (<>
        <Space>
            <Space size={4}>
                <Button icon={<DoubleLeftOutlined />} onClick={() => goToDate(-1)} />
                <DatePicker
                    id='date'
                    allowClear={false}
                    value={date ? dayjs(date) : null}
                    onChange={(d, _) => go(d.toDate(), type)}
                />
                <Button icon={<DoubleRightOutlined />} onClick={() => goToDate(1)} />
            </Space>

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

                <WodDayBarData type={type} date={date} country={country} />
                <WodItemTable type={type} date={date} country={country} />

            </Card>

        </>) : null}

    </>);
};

export default HomePage;
