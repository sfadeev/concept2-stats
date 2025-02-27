import { Card, DatePicker, Segmented, Space } from 'antd';
import dayjs from 'dayjs';
import { useState } from 'react';
import WodCalendarData from '../components/WodCalendarData';
import WodDayBarData from '../components/WodDayBarData';

export default () => {

    const [date, setDate] = useState<Date | null>(new Date());
    const [type, setType] = useState<string | null>('rowerg');

    return (<>

        <Space>
            <DatePicker value={date ? dayjs(date) : null} onChange={(date, dateString) => {
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
        </Space>

        {(date && type) ? (<>
            <Card size='small' style={{ marginTop: 20 }}>
                <div style={{ overflowX: 'auto', overflowY: 'hidden' }}>
                    <WodCalendarData
                        type={type}
                        year={date.getFullYear()}
                        onClick={(date, event) => {
                            return setDate(date);
                        }}
                    />
                </div>

                <WodDayBarData type={type} date={date} />
            </Card>

        </>) : null}

    </>);
};
