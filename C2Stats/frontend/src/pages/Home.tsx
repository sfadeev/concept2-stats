import WodCalendarData from '../components/WodCalendarData';
import WodDayBarData from '../components/WodDayBarData';

export default () => {

    return (
        <div>
            <WodCalendarData wodType='rowerg' year={2025} />
            <WodDayBarData wodType='rowerg' />
        </div>
    );
};
