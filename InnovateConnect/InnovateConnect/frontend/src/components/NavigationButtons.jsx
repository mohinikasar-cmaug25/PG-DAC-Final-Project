import { useNavigate } from 'react-router-dom';
import { FiArrowLeft, FiArrowRight } from 'react-icons/fi';

const NavigationButtons = ({ className = "" }) => {
    const navigate = useNavigate();

    return (
        <div className={`flex items-center gap-2 ${className}`}>
            <button
                onClick={() => navigate(-1)}
                className="p-2 rounded-full bg-white/80 border border-gray-100 text-gray-400 hover:text-primary hover:bg-white hover:shadow-sm transition-all flex items-center justify-center group"
                title="Go Back"
            >
                <FiArrowLeft className="group-hover:-translate-x-0.5 transition-transform" />
            </button>
            <button
                onClick={() => navigate(1)}
                className="p-2 rounded-full bg-white/80 border border-gray-100 text-gray-400 hover:text-primary hover:bg-white hover:shadow-sm transition-all flex items-center justify-center group"
                title="Go Forward"
            >
                <FiArrowRight className="group-hover:translate-x-0.5 transition-transform" />
            </button>
        </div>
    );
};

export default NavigationButtons;
