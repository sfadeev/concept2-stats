import WodCalendarData from '../components/WodCalendarData';

export default () => {

    return (
        <div>
            <h3>rowerg</h3>
            <WodCalendarData year={2025} wodType='rowerg' />

            <h3>bikeerg</h3>
            <WodCalendarData year={2025} wodType='bikeerg' />

            <h3>skierg</h3>
            <WodCalendarData year={2025} wodType='skierg' />

            {/* <WodDayBarData wodType='rowerg' /> */}
        </div>
    );
};
