import Sidebar from './Sidebar';
import { Outlet } from 'react-router-dom';
import { FiMenu } from 'react-icons/fi';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import NavigationButtons from './NavigationButtons';

const DashboardLayout = ({ children }) => {
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

    return (
        <div className="min-h-screen bg-gray-50 flex">
            {/* Desktop Sidebar */}
            <Sidebar />

            {/* Main Content Area */}
            <main className="flex-1 md:ml-64 min-h-screen transition-all">
                {/* Mobile Header */}
                <header className="bg-white shadow-sm p-4 md:hidden flex justify-between items-center sticky top-0 z-30">
                    <Link to="/" className="flex items-center gap-2 text-xl font-bold text-primary">
                        <div className="w-8 h-8 bg-white rounded-full shadow-sm flex items-center justify-center p-1 border border-gray-100">
                            <img src="/images/logo.png" alt="Logo" className="w-full h-full object-contain rounded-full" />
                        </div>
                        <span>Innovate<span className="text-dark">Connect</span></span>
                    </Link>
                    <div className="flex items-center gap-4">
                        <NavigationButtons />
                        <button onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)} className="text-2xl text-dark">
                            <FiMenu />
                        </button>
                    </div>
                </header>

                {/* Mobile Sidebar Overlay */}
                {isMobileMenuOpen && (
                    <div className="fixed inset-0 z-50 bg-black/50 md:hidden" onClick={() => setIsMobileMenuOpen(false)}>
                        <Sidebar className="flex h-full shadow-2xl" />
                    </div>
                )}

                <div className="p-4 md:p-8 max-w-7xl mx-auto">
                    <NavigationButtons className="mb-6 hidden md:flex" />
                    {children}
                </div>
            </main>
        </div>
    );
};

export default DashboardLayout;
