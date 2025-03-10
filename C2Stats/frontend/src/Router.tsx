import { BrowserRouter, Route, Routes } from 'react-router-dom';
import Home from './pages/Home';

export default () => {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/:qdate?/:qtype?" element={<Home />} />
            </Routes>
        </BrowserRouter>
    );
};
