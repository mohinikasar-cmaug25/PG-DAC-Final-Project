import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { FiMenu, FiX } from 'react-icons/fi';
import { useState } from 'react';
import NavigationButtons from './NavigationButtons';

const Navbar = () => {
    const { user } = useAuth();
    const [isOpen, setIsOpen] = useState(false);

    return (
        <nav className="bg-white shadow-md sticky top-0 z-50">
            <div className="container mx-auto px-6 py-4 flex justify-between items-center">
                <div className="flex items-center gap-6">
                    <NavigationButtons className="hidden sm:flex" />
                    <Link to="/" className="flex items-center gap-3 group">
                        <div className="w-10 h-10 bg-white rounded-full shadow-sm flex items-center justify-center p-1 border border-gray-100 group-hover:shadow-md transition-all">
                            <img src="/images/logo.png" alt="InnovateConnect Logo" className="w-full h-full object-contain rounded-full" />
                        </div>
                        <span className="text-2xl font-bold text-primary group-hover:text-secondary transition-colors">
                            Innovate<span className="text-dark">Connect</span>
                        </span>
                    </Link>
                </div>

                {/* Desktop Menu */}
                <div className="hidden md:flex items-center gap-8">
                    <Link to="/" className="text-dark hover:text-primary font-medium transition-colors">Home</Link>
                    <Link to="/about" className="text-dark hover:text-primary font-medium transition-colors">About Us</Link>
                    <Link to="/contact" className="text-dark hover:text-primary font-medium transition-colors">Contact Us</Link>

                    <Link to="/login" className="px-5 py-2 text-primary font-bold hover:bg-primary/5 rounded-full transition-all border border-primary/20 hover:border-primary">
                        Sign In
                    </Link>
                    <Link to="/register" className="px-6 py-2.5 bg-primary text-white rounded-full font-bold hover:bg-secondary transition-all shadow-md hover:shadow-primary/30 transform hover:-translate-y-0.5">
                        Join Us
                    </Link>

                </div>

                {/* Mobile Menu Button */}
                <button className="md:hidden text-2xl text-dark" onClick={() => setIsOpen(!isOpen)}>
                    {isOpen ? <FiX /> : <FiMenu />}
                </button>
            </div>

            {/* Mobile Dropdown */}
            {isOpen && (
                <div className="md:hidden bg-white border-t p-4 flex flex-col gap-4 shadow-lg">
                    <Link to="/" className="text-dark hover:text-primary" onClick={() => setIsOpen(false)}>Home</Link>
                    <Link to="/about" className="text-dark hover:text-primary" onClick={() => setIsOpen(false)}>About Us</Link>
                    <Link to="/contact" className="text-dark hover:text-primary" onClick={() => setIsOpen(false)}>Contact Us</Link>
                    <Link to="/login" className="text-primary font-bold px-4 py-2 border border-primary/20 rounded-xl bg-primary/5" onClick={() => setIsOpen(false)}>Sign In</Link>
                    <Link to="/register" className="bg-primary text-white font-bold px-4 py-2 rounded-xl text-center" onClick={() => setIsOpen(false)}>Join Us</Link>

                </div>
            )}
        </nav>
    );
};
export default Navbar;
