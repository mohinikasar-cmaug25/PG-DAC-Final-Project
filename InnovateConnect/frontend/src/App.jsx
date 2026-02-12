import { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation, Link, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Navbar from './components/Navbar';
import DashboardLayout from './components/DashboardLayout';
import Footer from './components/Footer';
import Login from './pages/Login';
import Register from './pages/Register';
import AboutUs from './pages/AboutUs';
import ContactUs from './pages/ContactUs';
import UserProfile from './pages/UserProfile';
import Settings from './pages/Settings';
import Resources from './pages/Resources';
import StudentDashboard from './pages/StudentDashboard';
import CompanyDashboard from './pages/CompanyDashboard';
import AdminDashboard from './pages/AdminDashboard';
import ProtectedRoute from './components/ProtectedRoute';
import { FiTrendingUp, FiZap, FiUsers, FiCheckCircle, FiArrowRight } from 'react-icons/fi';

// Home Component
const Home = () => {
  const { user } = useAuth();
  return (
    <div className="flex flex-col min-h-screen selection:bg-primary/30">
      {/* Hero Section */}
      <div className="flex-1 flex flex-col items-center justify-center bg-light text-center px-4 pt-16 pb-24 relative overflow-hidden">
        {/* Background blobs */}
        <div className="absolute -top-24 -left-24 w-[500px] h-[500px] bg-primary/10 rounded-full mix-blend-multiply filter blur-[100px] animate-pulse"></div>
        <div className="absolute top-1/4 -right-24 w-[400px] h-[400px] bg-accent/10 rounded-full mix-blend-multiply filter blur-[100px] animate-pulse animation-delay-2000"></div>
        <div className="absolute -bottom-24 left-1/4 w-[450px] h-[450px] bg-purple-500/10 rounded-full mix-blend-multiply filter blur-[100px] animate-pulse animation-delay-4000"></div>

        <div className="max-w-7xl mx-auto z-10">
          <div className="flex flex-col lg:flex-row items-center gap-16 mb-24">
            <div className="text-left lg:w-3/5">
              <div className="inline-flex items-center gap-2 px-4 py-2 bg-primary/10 text-primary rounded-full text-sm font-bold mb-6 animate-fade-in">
                <FiZap className="animate-bounce" />
                <span>Revolutionizing Student-Industry Synergy</span>
              </div>
              <h1 className="text-6xl md:text-8xl font-black text-dark mb-8 leading-[1.1] tracking-tight animate-fade-in">
                Where <span className="text-transparent bg-clip-text bg-gradient-to-r from-primary via-purple-600 to-accent">Ideas</span> <br />
                Meet <span className="text-transparent bg-clip-text bg-gradient-to-r from-secondary to-blue-600">Industry</span>
              </h1>
              <p className="text-2xl text-gray-500 mb-10 leading-relaxed max-w-2xl">
                Innovate Connect bridges the gap between talented students and forward-thinking companies.
                Share ideas, find internships, and build the future together.
              </p>
              <div className="flex flex-col sm:flex-row gap-6">
                <Link to="/login" className="px-10 py-5 bg-white text-dark text-xl rounded-2xl font-bold hover:bg-gray-50 transition-all border-2 border-gray-100 shadow-sm hover:shadow-md flex items-center justify-center gap-2">
                  Sign In
                </Link>
                <Link to="/register" className="group px-10 py-5 bg-primary text-white text-xl rounded-2xl font-bold hover:bg-secondary transition-all shadow-xl hover:shadow-primary/40 transform hover:-translate-y-1 flex items-center justify-center gap-2">
                  Join Us <FiArrowRight className="group-hover:translate-x-1 transition-transform" />
                </Link>
              </div>
            </div>
            <div className="lg:w-2/5 relative">
              <div className="absolute inset-0 bg-gradient-to-br from-primary to-accent rounded-3xl transform rotate-3 opacity-10 blur-2xl"></div>
              <div className="relative group p-4">
                <div className="absolute -inset-1 bg-gradient-to-r from-primary to-accent rounded-[2rem] blur opacity-25 group-hover:opacity-50 transition duration-1000 group-hover:duration-200"></div>
                <img
                  src="/images/hero.png"
                  alt="Collaboration"
                  className="relative rounded-[1.5rem] shadow-2xl transform transition-transform hover:scale-[1.02] duration-700 border-8 border-white"
                />
              </div>
            </div>
          </div>

          {/* Features Grid */}
          <div className="grid md:grid-cols-3 gap-10 mt-12 text-left">
            {[
              {
                icon: <FiTrendingUp className="text-blue-600" />,
                title: "Launch Career",
                desc: "Find premium internships that match your specific skills and passion for technology.",
                bg: "bg-blue-50"
              },
              {
                icon: <FiZap className="text-pink-600" />,
                title: "Share Ideas",
                desc: "Get discovered by top tier companies looking for fresh innovation and new perspectives.",
                bg: "bg-pink-50"
              },
              {
                icon: <FiUsers className="text-purple-600" />,
                title: "Direct Connect",
                desc: "Direct networking between students and industry leaders without the middleman.",
                bg: "bg-purple-50"
              }
            ].map((feature, i) => (
              <div key={i} className="group bg-white p-10 rounded-[2.5rem] shadow-sm border border-gray-100 hover:shadow-2xl hover:shadow-primary/10 transition-all duration-500 hover:-translate-y-2 relative overflow-hidden">
                <div className={`w-16 h-16 ${feature.bg} rounded-2xl flex items-center justify-center mb-8 text-3xl shadow-inner group-hover:scale-110 transition-transform duration-500`}>
                  {feature.icon}
                </div>
                <h3 className="font-black text-3xl mb-4 text-dark">{feature.title}</h3>
                <p className="text-gray-500 text-lg leading-relaxed">{feature.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Testimonials Section */}
      <div className="w-full bg-dark py-32 px-4 relative overflow-hidden">
        <div className="absolute top-0 left-0 w-full h-20 bg-gradient-to-b from-light to-transparent"></div>
        <div className="max-w-6xl mx-auto flex flex-col lg:flex-row items-center gap-24 relative z-10">
          <div className="lg:w-1/2 relative">
            <div className="absolute -inset-4 bg-primary/20 rounded-3xl blur-2xl"></div>
            <img
              src="/images/success.png"
              alt="Student Success"
              className="relative rounded-3xl shadow-2xl border-4 border-gray-800 transform -rotate-2 hover:rotate-0 transition-all duration-700"
            />
          </div>
          <div className="lg:w-1/2 text-left">
            <div className="flex items-center gap-2 text-primary font-bold tracking-widest uppercase text-sm mb-6">
              <span className="w-12 h-[2px] bg-primary"></span>
              SUCCESS STORIES
            </div>
            <h2 className="text-5xl font-black text-white mb-8 leading-tight">From Campus to <span className="text-primary italic">Global Career</span></h2>
            <div className="bg-gray-900/50 backdrop-blur-xl border border-white/10 p-10 rounded-[2rem] shadow-2xl relative">
              <FiCheckCircle className="absolute -top-6 -right-6 text-5xl text-primary drop-shadow-lg" />
              <blockquote className="text-2xl text-gray-300 italic mb-10 leading-relaxed font-medium">
                "Innovate Connect helped me find an internship that perfectly aligned with my skills. The projects I worked on gave me the confidence to land a full-time role at a top tech company."
              </blockquote>
              <div className="flex items-center gap-5">
                <div className="w-16 h-16 bg-gradient-to-br from-primary to-accent rounded-2xl overflow-hidden border-2 border-white/20 p-0.5">
                  <img src="https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=200&h=200&fit=crop" alt="Student" className="rounded-[0.9rem]" />
                </div>
                <div>
                  <h4 className="font-bold text-white text-xl">Sarah Miller</h4>
                  <p className="text-primary font-semibold">Software Engineer @ TechFlow</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// Dashboard Router
const DashboardRouter = () => {
  const { user } = useAuth();

  if (!user) return <Navigate to="/login" replace />;
  if (user.role === 'Student') return <StudentDashboard />;
  if (user.role === 'Company') return <CompanyDashboard />;
  if (user.role === 'Admin') return <AdminDashboard />;

  return <div className="p-10 text-center">
    <h2 className="text-xl font-bold">Unknown Role</h2>
    <p>Please contact support if you believe this is an error.</p>
  </div>;
};

// App Layout
const AppLayout = () => {
  const location = useLocation();
  const isDashboard = location.pathname.startsWith('/dashboard') ||
    ['/profile', '/settings', '/resources', '/post-internship', '/candidates', '/applications'].some(path => location.pathname.startsWith(path)) ||
    location.pathname.startsWith('/admin');

  const { user, loading } = useAuth();

  useEffect(() => {
    if (!loading) {
      console.log("Current Auth State:", { user, path: location.pathname });
    }
  }, [user, loading, location.pathname]);

  return (
    <div className="min-h-screen bg-light text-dark font-sans flex flex-col">
      {!isDashboard && <Navbar />}
      <div className="flex-grow">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/about" element={<AboutUs />} />
          <Route path="/contact" element={<ContactUs />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          <Route path="/dashboard" element={
            <ProtectedRoute>
              <DashboardLayout>
                <DashboardRouter />
              </DashboardLayout>
            </ProtectedRoute>
          } />

          <Route path="/profile" element={
            <ProtectedRoute>
              <DashboardLayout>
                <UserProfile />
              </DashboardLayout>
            </ProtectedRoute>
          } />

          <Route path="/settings" element={
            <ProtectedRoute>
              <DashboardLayout>
                <Settings />
              </DashboardLayout>
            </ProtectedRoute>
          } />

          <Route path="/resources" element={
            <ProtectedRoute>
              <DashboardLayout>
                <Resources />
              </DashboardLayout>
            </ProtectedRoute>
          } />

          <Route path="/post-internship" element={
            <ProtectedRoute allowedRoles={['Company']}>
              <DashboardLayout>
                <CompanyDashboard initialTab="post-internship" />
              </DashboardLayout>
            </ProtectedRoute>
          } />

          <Route path="/candidates" element={
            <ProtectedRoute allowedRoles={['Company', 'Admin']}>
              <DashboardLayout>
                <div className="p-10 text-center">
                  <h2 className="text-2xl font-bold mb-4">Candidates</h2>
                  <p className="text-gray-500">Feature coming soon.</p>
                </div>
              </DashboardLayout>
            </ProtectedRoute>
          } />

          <Route path="/admin/users" element={
            <ProtectedRoute allowedRoles={['Admin']}>
              <DashboardLayout>
                <div className="p-10 text-center">
                  <h2 className="text-2xl font-bold mb-4">User Management</h2>
                  <p className="text-gray-500">Admin User Management Interface</p>
                </div>
              </DashboardLayout>
            </ProtectedRoute>
          } />

          <Route path="/admin/moderation" element={
            <ProtectedRoute allowedRoles={['Admin']}>
              <DashboardLayout>
                <div className="p-10 text-center">
                  <h2 className="text-2xl font-bold mb-4">Content Moderation</h2>
                  <p className="text-gray-500">Admin Moderation Interface</p>
                </div>
              </DashboardLayout>
            </ProtectedRoute>
          } />

          <Route path="/applications" element={
            <ProtectedRoute allowedRoles={['Student', 'Admin']}>
              <DashboardLayout>
                <StudentDashboard initialTab="applications" />
              </DashboardLayout>
            </ProtectedRoute>
          } />
        </Routes>
      </div>
      {!isDashboard && <Footer />}
    </div>
  );
};


// Main App Component
function App() {
  return (
    <AuthProvider>
      <Router>
        <AppLayout />
      </Router>
    </AuthProvider>
  );
}

export default App;
