import { useState } from 'react';
import WodCalendarData from '../components/WodCalendarData';
import WodDayBarData from '../components/WodDayBarData';

export default () => {

    const [date, setDate] = useState<Date | null>(null);

    return (
        <div>
            <h3>rowerg</h3>
            <WodCalendarData year={2025} wodType='rowerg'
                onClick={(d, x) => setDate(d)}
            />

            <WodDayBarData wodType='rowerg' date={date} />

            <h3>bikeerg</h3>
            <WodCalendarData year={2025} wodType='bikeerg' />

            <h3>skierg</h3>
            <WodCalendarData year={2025} wodType='skierg' />

        </div>
    );
};
