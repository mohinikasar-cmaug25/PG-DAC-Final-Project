import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { FiUsers, FiBriefcase, FiZap, FiActivity, FiTrash2, FiShield, FiCheckCircle } from 'react-icons/fi';
import axios from 'axios';

const AdminDashboard = () => {
    const { token } = useAuth();
    const [stats, setStats] = useState(null);
    const [users, setUsers] = useState([]);
    const [ideas, setIdeas] = useState([]);
    const [internships, setInternships] = useState([]);
    const [contacts, setContacts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState('users');


    const fetchData = async () => {
        if (!token) return;
        setLoading(true);
        try {
            const config = { headers: { Authorization: `Bearer ${token}` } };
            const [statsRes, usersRes, ideasRes, internsRes, contactsRes] = await Promise.all([
                axios.get('http://localhost:5098/api/admin/stats', config),
                axios.get('http://localhost:5098/api/admin/users', config),
                axios.get('http://localhost:5098/api/admin/ideas', config),
                axios.get('http://localhost:5098/api/admin/internships', config),
                axios.get('http://localhost:5098/api/admin/contacts', config)
            ]);

            setStats(statsRes.data);
            setUsers(usersRes.data);
            setIdeas(ideasRes.data);
            setInternships(internsRes.data);
            setContacts(contactsRes.data);
        } catch (error) {
            console.error('Fetch error:', error);
            if (error.response?.status === 401 || error.response?.status === 403) {
                // Handle unauthorized/forbidden (maybe token expired)
                console.error("Session expired or unauthorized access.");
            }
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, [token]);

    const handleDeleteUser = async (userId) => {
        console.log('Delete user clicked, userId:', userId);
        if (!window.confirm("Are you sure you want to delete this user? This will also remove all their posted ideas, internships, and applications. This action cannot be undone.")) {
            console.log('Delete cancelled by user');
            return;
        }

        try {
            console.log('Attempting to delete user:', userId);
            const config = { headers: { Authorization: `Bearer ${token}` } };
            await axios.delete(`http://localhost:5098/api/admin/users/${userId}`, config);
            alert("User deleted successfully.");
            fetchData(); // Refresh everything
        } catch (error) {
            console.error('Delete error:', error);
            alert("Failed to delete user: " + (error.response?.data || error.message));
        }
    };

    const handleDeleteIdea = async (ideaId) => {
        if (!window.confirm("Are you sure you want to delete this idea?")) return;

        try {
            const config = { headers: { Authorization: `Bearer ${token}` } };
            await axios.delete(`http://localhost:5098/api/admin/ideas/${ideaId}`, config);
            alert("Idea deleted successfully.");
            fetchData();
        } catch (error) {
            alert("Failed to delete idea: " + (error.response?.data || error.message));
        }
    };

    const handleDeleteInternship = async (internshipId) => {
        if (!window.confirm("Are you sure you want to delete this internship? This will also remove all applications for it.")) return;

        try {
            const config = { headers: { Authorization: `Bearer ${token}` } };
            await axios.delete(`http://localhost:5098/api/admin/internships/${internshipId}`, config);
            alert("Internship and related applications deleted successfully.");
            fetchData();
        } catch (error) {
            alert("Failed to delete internship: " + (error.response?.data || error.message));
        }
    };

    const handleDeleteContact = async (contactId) => {
        if (!window.confirm("Are you sure you want to delete this message?")) return;

        try {
            const config = { headers: { Authorization: `Bearer ${token}` } };
            await axios.delete(`http://localhost:5098/api/admin/contacts/${contactId}`, config);
            alert("Message deleted successfully.");
            fetchData();
        } catch (error) {
            alert("Failed to delete message: " + (error.response?.data || error.message));
        }
    };

    const handleToggleReview = async (contactId) => {
        try {
            const config = { headers: { Authorization: `Bearer ${token}` } };
            await axios.put(`http://localhost:5098/api/admin/contacts/${contactId}/review`, {}, config);
            fetchData();
        } catch (error) {
            console.error('Error toggling review status:', error);
            alert("Failed to update status: " + (error.response?.data || error.message));
        }
    };

    if (loading && !stats) return <div className="p-10 text-center">Loading Dashboard...</div>;

    const statsConfig = [
        { label: 'Total Students', value: stats?.totalStudents || 0, icon: FiUsers, color: 'text-blue-600', bg: 'bg-blue-100' },
        { label: 'Total Companies', value: stats?.totalCompanies || 0, icon: FiBriefcase, color: 'text-purple-600', bg: 'bg-purple-100' },
        { label: 'Total Ideas', value: stats?.totalIdeas || 0, icon: FiZap, color: 'text-yellow-600', bg: 'bg-yellow-100' },
        { label: 'Active Internships', value: stats?.activeInternships || 0, icon: FiActivity, color: 'text-green-600', bg: 'bg-green-100' },
    ];

    return (
        <div className="container mx-auto px-4 py-8">
            <div className="bg-white p-6 rounded-xl shadow-md mb-8 border border-gray-100 flex items-center gap-4">
                <div className="p-3 bg-primary/10 rounded-lg text-primary">
                    <FiShield size={32} />
                </div>
                <div>
                    <h1 className="text-2xl font-bold text-dark">Admin System Control</h1>
                    <p className="text-gray-500">Manage users, ideas, and internships efficiently</p>
                </div>
            </div>

            {/* Stats Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8 animate-fade-in">
                {statsConfig.map((stat, index) => (
                    <div key={index} className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex items-center justify-between hover:shadow-md transition-shadow">
                        <div>
                            <p className="text-gray-500 text-sm font-medium">{stat.label}</p>
                            <h3 className="text-3xl font-bold text-dark mt-1">{stat.value}</h3>
                        </div>
                        <div className={`p-3 rounded-full ${stat.bg} ${stat.color}`}>
                            <stat.icon size={24} />
                        </div>
                    </div>
                ))}
            </div>

            {/* Tabs */}
            <div className="flex gap-4 mb-6 border-b border-gray-100">
                {['users', 'ideas', 'internships', 'messages'].map((tab) => (
                    <button
                        key={tab}
                        onClick={() => setActiveTab(tab)}
                        className={`pb-4 px-2 font-semibold capitalize transition-all ${activeTab === tab ? 'text-primary border-b-2 border-primary' : 'text-gray-400 hover:text-gray-600'}`}
                    >
                        {tab}
                    </button>
                ))}
            </div>

            {/* Content Sections */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden min-h-[400px]">
                {loading && <div className="p-10 text-center text-gray-400">Refreshing data...</div>}

                {!loading && activeTab === 'users' && (
                    <div className="overflow-x-auto">
                        <table className="w-full text-left">
                            <thead className="bg-gray-50 text-gray-500 text-sm uppercase font-semibold">
                                <tr>
                                    <th className="px-6 py-4">Name / Email</th>
                                    <th className="px-6 py-4">Role</th>
                                    <th className="px-6 py-4">Joined Date</th>
                                    <th className="px-6 py-4 text-right">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {users.map((u) => (
                                    <tr key={u.id || u.Id} className="hover:bg-gray-50 transition-colors">
                                        <td className="px-6 py-4">
                                            <div className="font-semibold text-dark">{u.profileName}</div>
                                            <div className="text-sm text-gray-500">{u.email}</div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <span className={`px-3 py-1 rounded-full text-xs font-bold ${u.role === 'Admin' ? 'bg-red-100 text-red-600' :
                                                u.role === 'Company' ? 'bg-purple-100 text-purple-600' :
                                                    'bg-blue-100 text-blue-600'
                                                }`}>
                                                {u.role}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 text-sm text-gray-500">
                                            {new Date(u.createdAt).toLocaleDateString()}
                                        </td>
                                        <td className="px-6 py-4 text-right">
                                            {u.role !== 'Admin' && (
                                                <button
                                                    onClick={() => handleDeleteUser(u.id || u.Id)}
                                                    className="p-2 text-gray-400 hover:text-red-600 transition-colors"
                                                    title="Delete User"
                                                >
                                                    <FiTrash2 size={20} />
                                                </button>
                                            )}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}

                {!loading && activeTab === 'ideas' && (
                    <div className="overflow-x-auto">
                        <table className="w-full text-left">
                            <thead className="bg-gray-50 text-gray-500 text-sm uppercase font-semibold">
                                <tr>
                                    <th className="px-6 py-4">Title / Student</th>
                                    <th className="px-6 py-4">Technology</th>
                                    <th className="px-6 py-4">Posted Date</th>
                                    <th className="px-6 py-4 text-right">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {ideas.map((idea) => (
                                    <tr key={idea.id} className="hover:bg-gray-50 transition-colors">
                                        <td className="px-6 py-4">
                                            <div className="font-semibold text-dark">{idea.title}</div>
                                            <div className="text-sm text-gray-500">By {idea.studentName}</div>
                                        </td>
                                        <td className="px-6 py-4 text-sm text-gray-600">{idea.technologyUsed}</td>
                                        <td className="px-6 py-4 text-sm text-gray-500">
                                            {new Date(idea.postedDate).toLocaleDateString()}
                                        </td>
                                        <td className="px-6 py-4 text-right">
                                            <button
                                                onClick={() => handleDeleteIdea(idea.id)}
                                                className="p-2 text-gray-400 hover:text-red-600 transition-colors"
                                                title="Delete Idea"
                                            >
                                                <FiTrash2 size={20} />
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                                {ideas.length === 0 && (
                                    <tr>
                                        <td colSpan="4" className="text-center py-10 text-gray-400">No ideas posted yet.</td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                )}

                {!loading && activeTab === 'internships' && (
                    <div className="overflow-x-auto">
                        <table className="w-full text-left">
                            <thead className="bg-gray-50 text-gray-500 text-sm uppercase font-semibold">
                                <tr>
                                    <th className="px-6 py-4">Title / Company</th>
                                    <th className="px-6 py-4">Stipend</th>
                                    <th className="px-6 py-4">Posted Date</th>
                                    <th className="px-6 py-4 text-right">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {internships.map((intern) => (
                                    <tr key={intern.id} className="hover:bg-gray-50 transition-colors">
                                        <td className="px-6 py-4">
                                            <div className="font-semibold text-dark">{intern.title}</div>
                                            <div className="text-sm text-gray-500">{intern.companyName}</div>
                                        </td>
                                        <td className="px-6 py-4 text-sm text-gray-600">₹{intern.stipend}</td>
                                        <td className="px-6 py-4 text-sm text-gray-500">
                                            {new Date(intern.postedDate).toLocaleDateString()}
                                        </td>
                                        <td className="px-6 py-4 text-right">
                                            <button
                                                onClick={() => handleDeleteInternship(intern.id)}
                                                className="p-2 text-gray-400 hover:text-red-600 transition-colors"
                                                title="Delete Internship"
                                            >
                                                <FiTrash2 size={20} />
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                                {internships.length === 0 && (
                                    <tr>
                                        <td colSpan="4" className="text-center py-10 text-gray-400">No internships posted yet.</td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                )}

                {!loading && activeTab === 'messages' && (
                    <div className="overflow-x-auto">
                        <table className="w-full text-left">
                            <thead className="bg-gray-50 text-gray-500 text-sm uppercase font-semibold">
                                <tr>
                                    <th className="px-6 py-4">Sender</th>
                                    <th className="px-6 py-4">Message</th>
                                    <th className="px-6 py-4">Status</th>
                                    <th className="px-6 py-4">Date</th>
                                    <th className="px-6 py-4 text-right">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {contacts.map((msg) => (
                                    <tr key={msg.id} className="hover:bg-gray-50 transition-colors">
                                        <td className="px-6 py-4">
                                            <div className="font-semibold text-dark">{msg.name}</div>
                                            <div className="text-sm text-gray-500">{msg.email}</div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <p className="text-sm text-gray-600 max-w-md whitespace-pre-wrap">{msg.message}</p>
                                        </td>
                                        <td className="px-6 py-4">
                                            <span className={`px-2 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider ${msg.isReviewed ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'}`}>
                                                {msg.isReviewed ? 'Reviewed' : 'Pending'}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 text-sm text-gray-500 whitespace-nowrap">
                                            {new Date(msg.createdAt).toLocaleDateString()}
                                        </td>
                                        <td className="px-6 py-4 text-right flex items-center justify-end gap-2">
                                            <button
                                                onClick={() => handleToggleReview(msg.id)}
                                                className={`p-2 rounded-lg transition-colors ${msg.isReviewed ? 'text-gray-400 hover:text-yellow-600' : 'text-primary hover:text-secondary'}`}
                                                title={msg.isReviewed ? "Mark as Pending" : "Mark as Reviewed"}
                                            >
                                                <FiCheckCircle size={20} />
                                            </button>
                                            <button
                                                onClick={() => handleDeleteContact(msg.id)}
                                                className="p-2 text-gray-400 hover:text-red-600 transition-colors"
                                                title="Delete Message"
                                            >
                                                <FiTrash2 size={20} />
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                                {contacts.length === 0 && (
                                    <tr>
                                        <td colSpan="4" className="text-center py-10 text-gray-400">No messages received yet.</td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
};

export default AdminDashboard;
