
import React, { useState } from 'react';
import Pagination from '../../components/common/Pagination';
import { CompanyInfo, PaginationParams, SortingParams, CompanyInfoFilteringParams } from '../../types';
import { companyInfoService } from '../../services/companyInfoService';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import AddCompanyModal from '../../components/modals/AddCompanyModal';
import GenericDeleteConfirmationModal from '../../components/modals/GenericDeleteConfirmationModal';
import './CompaniesPage.scss';


const CompaniesPage: React.FC = () => {
  usePageTitle('Companies');

  // Paged companies
  const {
    data: companies,
    loading,
    error,
    pagination,
    setPagination,
    sorting,
    setSorting,
    filtering,
    setFiltering,
    totalCount,
  } = usePagedData<CompanyInfo, PaginationParams, SortingParams, CompanyInfoFilteringParams>({
    fetchData: companyInfoService.getCompanyInfos,
    initialPagination: { pageNumber: 1, pageSize: 10 },
    initialSorting: { orderBy: 'companyName', descending: false },
    initialFiltering: { searchTerm: '' },
  });

  const [showModal, setShowModal] = useState(false);
  const [selectedCompany, setSelectedCompany] = useState<CompanyInfo | undefined>();
  const [deleteLoading, setDeleteLoading] = useState<number | null>(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [companyToDelete, setCompanyToDelete] = useState<CompanyInfo | undefined>();


  const handleAddCompany = () => {
    setSelectedCompany(undefined);
    setShowModal(true);
  };

  const handleEditCompany = (company: CompanyInfo) => {
    setSelectedCompany(company);
    setShowModal(true);
  };

  // After add/edit, just reload the current page
  const handleCompanySaved = () => {
    setShowModal(false);
    setPagination({ ...pagination });
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
      setShowDeleteModal(false);
      setCompanyToDelete(undefined);
      setPagination({ ...pagination }); // reload
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to delete company');
      console.error('Error deleting company:', err);
    } finally {
      setDeleteLoading(null);
    }
  };


  const handleSetDefault = async (companyId: number) => {
    try {
      await companyInfoService.setDefaultCompany(companyId);
      setPagination({ ...pagination }); // reload
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to set default company');
      console.error('Error setting default company:', err);
    }
  };


  // Sorting icon helper
  const renderSortIcon = (column: string) => {
    if (sorting.orderBy !== column) return null;
    return (
      <i className={`bi ms-1 bi-sort-${sorting.descending ? 'down' : 'up'}-alt`}></i>
    );
  };

  // Sorting handler
  const handleSort = (column: string) => {
    setSorting({
      orderBy: column,
      descending: sorting.orderBy === column ? !sorting.descending : false
    });
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
        <button className="btn btn-sm btn-outline-danger ms-2" onClick={() => setPagination({ ...pagination })}>
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

          {/* Search and Add Controls */}
          <div className="row mb-4">
            <div className="col-md-6">
              <div className="input-group">
                <span className="input-group-text">
                  <i className="bi bi-search"></i>
                </span>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Search companies..."
                  value={filtering.searchTerm || ''}
                  onChange={e => setFiltering({ ...filtering, searchTerm: e.target.value })}
                />
              </div>
            </div>
            <div className="col-md-3"></div>
            <div className="col-md-3">
              <button
                className="btn btn-primary w-100"
                onClick={handleAddCompany}
              >
                <i className="bi bi-plus-circle me-2"></i>
                Add Company
              </button>
            </div>
          </div>

          <div className="card shadow-sm">
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-hover mb-0">
                  <thead className="table-dark">
                    <tr>
                      <th style={{ cursor: 'pointer' }} onClick={() => handleSort('companyName')}>
                        Company Name
                        {renderSortIcon('companyName')}
                      </th>
                      <th style={{ cursor: 'pointer' }} onClick={() => handleSort('legalName')}>
                        Legal Name
                        {renderSortIcon('legalName')}
                      </th>
                      <th style={{ cursor: 'pointer' }} onClick={() => handleSort('email')}>
                        Email
                        {renderSortIcon('email')}
                      </th>
                      <th style={{ cursor: 'pointer' }} onClick={() => handleSort('phone')}>
                        Phone
                        {renderSortIcon('phone')}
                      </th>
                      <th style={{ cursor: 'pointer' }} onClick={() => handleSort('currency')}>
                        Currency
                        {renderSortIcon('currency')}
                      </th>
                      <th>Status</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {companies.length === 0 ? (
                      <tr>
                        <td colSpan={7} className="text-center py-4">
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
                        </td>
                      </tr>
                    ) : (
                      companies.map(company => (
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
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          {/* Results count */}
          {totalCount > 0 && (
            <div className="mt-3 text-muted text-center">
              Showing {(pagination.pageNumber - 1) * pagination.pageSize + 1} to {Math.min(pagination.pageNumber * pagination.pageSize, totalCount)} of {totalCount} companies
            </div>
          )}

          {/* Pagination Controls */}
          <Pagination
            pageNumber={pagination.pageNumber}
            pageSize={pagination.pageSize}
            totalCount={totalCount}
            onPageChange={(page: number) => setPagination({ ...pagination, pageNumber: page })}
            ariaLabel="Company Pagination"
          />

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
      </div>
    </div>
  );
};

export default CompaniesPage;
