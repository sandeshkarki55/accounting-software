import React, { useState, useEffect } from 'react';
import { CompanyInfo } from '../../types/customers';
import { companyInfoService } from '../../services/api';
import { usePageTitle } from '../../hooks/usePageTitle';
import AddCompanyModal from '../../components/modals/AddCompanyModal';
import GenericDeleteConfirmationModal from '../../components/modals/GenericDeleteConfirmationModal';
import './CompaniesPage.scss';

const CompaniesPage: React.FC = () => {
  usePageTitle('Companies');

  const [companies, setCompanies] = useState<CompanyInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [selectedCompany, setSelectedCompany] = useState<CompanyInfo | undefined>();
  const [deleteLoading, setDeleteLoading] = useState<number | null>(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [companyToDelete, setCompanyToDelete] = useState<CompanyInfo | undefined>();

  useEffect(() => {
    loadCompanies();
  }, []);

  const loadCompanies = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await companyInfoService.getCompanyInfos();
      setCompanies(data);
    } catch (err) {
      setError('Failed to load companies');
      console.error('Error loading companies:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddCompany = () => {
    setSelectedCompany(undefined);
    setShowModal(true);
  };

  const handleEditCompany = (company: CompanyInfo) => {
    setSelectedCompany(company);
    setShowModal(true);
  };

  const handleCompanySaved = (savedCompany: CompanyInfo) => {
    if (selectedCompany) {
      // Update existing company
      setCompanies(prev => prev.map(company => 
        company.id === selectedCompany.id ? savedCompany : company
      ));
    } else {
      // Add new company
      setCompanies(prev => {
        // If the new company is set as default, update all others to not be default
        if (savedCompany.isDefault) {
          return [
            ...prev.map(company => ({ ...company, isDefault: false })),
            savedCompany
          ];
        }
        return [...prev, savedCompany];
      });
    }
    setShowModal(false);
  };

  const handleDeleteCompany = (company: CompanyInfo) => {
    setCompanyToDelete(company);
    setShowDeleteModal(true);
  };

  const handleConfirmDelete = async () => {
    if (!companyToDelete) return;

    try {
      setDeleteLoading(companyToDelete.id);
      await companyInfoService.deleteCompanyInfo(companyToDelete.id);
      setCompanies(prev => prev.filter(company => company.id !== companyToDelete.id));
      setShowDeleteModal(false);
      setCompanyToDelete(undefined);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete company');
      console.error('Error deleting company:', err);
    } finally {
      setDeleteLoading(null);
    }
  };

  const handleSetDefault = async (companyId: number) => {
    try {
      await companyInfoService.setDefaultCompany(companyId);
      // Update the companies list to reflect the new default
      setCompanies(prev => prev.map(company => ({
        ...company,
        isDefault: company.id === companyId
      })));
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to set default company');
      console.error('Error setting default company:', err);
    }
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{minHeight: '200px'}}>
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading companies...</span>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="alert alert-danger" role="alert">
        <i className="bi bi-exclamation-circle me-2"></i>
        {error}
        <button className="btn btn-sm btn-outline-danger ms-2" onClick={loadCompanies}>
          <i className="bi bi-arrow-clockwise me-1"></i>
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="container">
      <div className="row">
        <div className="col-12">
          <h1 className="mb-4 text-dark">Companies</h1>
          
          <div className="mb-4">
            <button 
              className="btn btn-primary"
              onClick={handleAddCompany}
            >
              <i className="bi bi-plus-circle me-2"></i>
              Add Company
            </button>
          </div>

          {companies.length === 0 ? (
            <div className="card shadow-sm">
              <div className="card-body">
                <div className="empty-state">
                  <div className="empty-state-icon">
                    <i className="bi bi-building"></i>
                  </div>
                  <h3>No Companies Found</h3>
                  <p>Get started by adding your first company for invoicing.</p>
                  <button 
                    className="btn btn-primary"
                    onClick={handleAddCompany}
                  >
                    <i className="bi bi-plus-circle me-2"></i>
                    Add Your First Company
                  </button>
                </div>
              </div>
            </div>
          ) : (
            <div className="card shadow-sm">
              <div className="card-body p-0">
                <div className="table-responsive">
                  <table className="table table-hover mb-0">
                    <thead className="table-dark">
                      <tr>
                        <th>Company Name</th>
                        <th>Legal Name</th>
                        <th>Email</th>
                        <th>Phone</th>
                        <th>Currency</th>
                        <th>Status</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {companies.map(company => (
                        <tr key={company.id}>
                          <td className="fw-bold">
                            {company.companyName}
                            {company.isDefault && (
                              <span className="badge bg-warning text-dark ms-2">
                                <i className="bi bi-star-fill me-1"></i>
                                Default
                              </span>
                            )}
                          </td>
                          <td>{company.legalName || '-'}</td>
                          <td>{company.email || '-'}</td>
                          <td>{company.phone || '-'}</td>
                          <td>{company.currency}</td>
                          <td>
                            <span className="badge bg-success">Active</span>
                          </td>
                          <td>
                            <div className="btn-group" role="group">
                              <button
                                className="btn btn-sm btn-outline-primary"
                                onClick={() => handleEditCompany(company)}
                                title="Edit Company"
                              >
                                <i className="bi bi-pencil"></i>
                              </button>
                              {!company.isDefault && (
                                <button
                                  className="btn btn-sm btn-outline-warning"
                                  onClick={() => handleSetDefault(company.id)}
                                  title="Set as Default"
                                >
                                  <i className="bi bi-star"></i>
                                </button>
                              )}
                              <button
                                className="btn btn-sm btn-outline-danger"
                                onClick={() => handleDeleteCompany(company)}
                                disabled={deleteLoading === company.id || company.isDefault}
                                title={company.isDefault ? "Cannot delete default company" : "Delete Company"}
                              >
                                {deleteLoading === company.id ? (
                                  <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                                ) : (
                                  <i className="bi bi-trash"></i>
                                )}
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Company Modal */}
      <AddCompanyModal
        show={showModal}
        onHide={() => setShowModal(false)}
        onCompanySaved={handleCompanySaved}
        company={selectedCompany}
      />

      {/* Delete Confirmation Modal */}
      <GenericDeleteConfirmationModal
        show={showDeleteModal}
        onHide={() => setShowDeleteModal(false)}
        onConfirm={handleConfirmDelete}
        itemName={companyToDelete?.companyName || ''}
        itemType="company"
        loading={deleteLoading !== null}
        warningMessage="This will soft delete the company. The company and its data will be preserved but hidden from normal views."
      />
    </div>
  );
};

export default CompaniesPage;
