import { useCallback, useEffect, useRef, useState, type FormEvent } from 'react';
import { CreateCustomerDialog } from './dialogs/create-customer-dialog';
import { CustomerDetailsDialog } from './dialogs/customer-details-dialog';
import { CustomerSuccessNotice } from './notifications/customer-success-notice';
import {
  describeCustomerListError,
  isAbortError,
} from './shared/customer-error-messages';
import { listCustomers } from './api/customers-api';
import { formatAuditDate, formatDate } from './shared/customer-format';
import type {
  CustomerDetails,
  CustomerListItem,
  PagedResponse,
} from './api/customer-contracts';
import './shared/customers.css';
import './customers-page.css';

const PAGE_SIZE = 13;

type CustomerListState =
  | Readonly<{ status: 'loading' }>
  | Readonly<{ status: 'success'; data: PagedResponse<CustomerListItem> }>
  | Readonly<{ status: 'error'; message: string }>;

export function CustomersPage() {
  const addCustomerButtonRef = useRef<HTMLButtonElement>(null);
  const detailsTriggerRef = useRef<HTMLButtonElement | null>(null);
  const focusAfterDialogRef = useRef<HTMLElement | null>(null);
  const selectAllCustomersRef = useRef<HTMLInputElement>(null);
  const [searchDraft, setSearchDraft] = useState('');
  const [search, setSearch] = useState('');
  const [searchError, setSearchError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [refreshKey, setRefreshKey] = useState(0);
  const [listState, setListState] = useState<CustomerListState>({ status: 'loading' });
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [selectedCustomerId, setSelectedCustomerId] = useState<number | null>(null);
  const [selectedCustomerIds, setSelectedCustomerIds] = useState<readonly number[]>([]);
  const [notice, setNotice] = useState<string | null>(null);
  const dismissNotice = useCallback(() => setNotice(null), []);

  useEffect(() => {
    const controller = new AbortController();

    listCustomers(
      {
        page,
        pageSize: PAGE_SIZE,
        search: search.length === 0 ? undefined : search,
      },
      controller.signal,
    )
      .then((data) => {
        if (data.totalPages > 0 && data.page > data.totalPages) {
          setPage(data.totalPages);
          return;
        }

        setListState({ status: 'success', data });
      })
      .catch((error: unknown) => {
        if (!isAbortError(error)) {
          setListState({ status: 'error', message: describeCustomerListError(error) });
        }
      });

    return () => controller.abort();
  }, [page, refreshKey, search]);

  useEffect(() => {
    if (isCreateDialogOpen || selectedCustomerId !== null) {
      return;
    }

    const requestedTarget = focusAfterDialogRef.current;
    if (requestedTarget === null) {
      return;
    }

    focusAfterDialogRef.current = null;
    const target = requestedTarget.isConnected ? requestedTarget : addCustomerButtonRef.current;
    target?.focus();
  }, [isCreateDialogOpen, selectedCustomerId]);

  function submitSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const nextSearch = searchDraft.trim();

    if (nextSearch.length > 100) {
      setSearchError('Search must not exceed 100 characters.');
      return;
    }

    setSearchError(null);
    setNotice(null);
    setSelectedCustomerIds([]);
    setPage(1);

    if (nextSearch === search) {
      setListState({ status: 'loading' });
      setRefreshKey((current) => current + 1);
    } else {
      setListState({ status: 'loading' });
      setSearch(nextSearch);
    }
  }

  function clearSearch() {
    setSearchDraft('');
    setSearchError(null);
    setNotice(null);

    if (search.length > 0) {
      setSelectedCustomerIds([]);
      setPage(1);
      setListState({ status: 'loading' });
      setSearch('');
    }
  }

  function handleCreated(customer: CustomerDetails) {
    setIsCreateDialogOpen(false);
    setSearchDraft('');
    setSearch('');
    setPage(1);
    setSelectedCustomerIds([]);
    setListState({ status: 'loading' });
    setRefreshKey((current) => current + 1);
    setSelectedCustomerId(customer.id);
    setNotice(`Customer ${formatFullName(customer)} created.`);
  }

  function handleChanged(customer: CustomerDetails) {
    setListState({ status: 'loading' });
    setRefreshKey((current) => current + 1);
    setNotice(`Customer ${formatFullName(customer)} updated.`);
  }

  function handleDeleted(customerId: number) {
    focusAfterDialogRef.current = addCustomerButtonRef.current;
    setSelectedCustomerId(null);
    setSelectedCustomerIds((current) => current.filter((id) => id !== customerId));
    setNotice(`Customer #${customerId} deleted from active records.`);

    if (
      listState.status === 'success'
      && listState.data.items.length === 1
      && page > 1
    ) {
      setPage((current) => current - 1);
    } else {
      setListState({ status: 'loading' });
      setRefreshKey((current) => current + 1);
    }
  }

  const listData = listState.status === 'success' ? listState.data : null;
  const visibleCustomerIds = listData?.items.map((customer) => customer.id) ?? [];
  const selectedVisibleCustomerCount = visibleCustomerIds.filter(
    (customerId) => selectedCustomerIds.includes(customerId),
  ).length;
  const allVisibleCustomersSelected = visibleCustomerIds.length > 0
    && selectedVisibleCustomerCount === visibleCustomerIds.length;
  const someVisibleCustomersSelected = selectedVisibleCustomerCount > 0
    && !allVisibleCustomersSelected;

  useEffect(() => {
    if (selectAllCustomersRef.current !== null) {
      selectAllCustomersRef.current.indeterminate = someVisibleCustomersSelected;
    }
  }, [someVisibleCustomersSelected]);

  function toggleCustomerSelection(customerId: number) {
    setSelectedCustomerIds((current) => (
      current.includes(customerId)
        ? current.filter((id) => id !== customerId)
        : [...current, customerId]
    ));
  }

  function toggleVisibleCustomerSelection(isChecked: boolean) {
    setSelectedCustomerIds((current) => {
      if (isChecked) {
        return Array.from(new Set([...current, ...visibleCustomerIds]));
      }

      return current.filter((id) => !visibleCustomerIds.includes(id));
    });
  }

  function clearCustomerSelection() {
    setSelectedCustomerIds([]);
    selectAllCustomersRef.current?.focus();
  }

  return (
    <section className="customers-panel" aria-labelledby="customer-directory-title">
      <header className="customers-panel-header">
        <h2 id="customer-directory-title">Customer Directory</h2>
        <form className="customer-search" role="search" onSubmit={submitSearch}>
          <label className="customer-sr-only" htmlFor="customer-search-input">
            Search customers
          </label>
          <div className="customer-search-controls">
            <input
              id="customer-search-input"
              type="search"
              value={searchDraft}
              maxLength={100}
              placeholder="Name or phone"
              aria-invalid={searchError !== null}
              aria-describedby={searchError === null ? 'customer-search-help' : 'customer-search-help customer-search-error'}
              onChange={(event) => {
                const nextSearchDraft = event.target.value;
                const isActiveSearchCleared = nextSearchDraft.trim().length === 0
                  && search.length > 0;
                setSearchDraft(isActiveSearchCleared ? '' : nextSearchDraft);
                setSearchError(null);

                if (isActiveSearchCleared) {
                  setNotice(null);
                  setSelectedCustomerIds([]);
                  setPage(1);
                  setListState({ status: 'loading' });
                  setSearch('');
                }
              }}
            />
            {searchDraft.length > 0 || search.length > 0 ? (
              <button
                className="customer-search-clear"
                type="button"
                aria-label="Clear customer search"
                onClick={clearSearch}
              >
                ×
              </button>
            ) : null}
            <button
              className="customer-search-submit"
              type="submit"
              aria-label="Search customers"
            >
              <svg aria-hidden="true" viewBox="0 0 24 24">
                <circle cx="11" cy="11" r="6.5" />
                <path d="m16 16 4 4" />
              </svg>
            </button>
          </div>
          <span className="customer-sr-only" id="customer-search-help">
            Names and phone numbers support partial matches.
          </span>
          {searchError !== null ? (
            <small className="customer-field-error" id="customer-search-error" role="alert">
              {searchError}
            </small>
          ) : null}
        </form>
      </header>

      <section className="customers-quick-actions" aria-labelledby="customer-quick-actions-title">
        <h3 id="customer-quick-actions-title">Quick Actions</h3>
        <div className="customers-quick-actions-buttons">
          {selectedCustomerIds.length > 0 ? (
            <>
              <button
                className="customer-button customer-button--secondary"
                type="button"
                disabled
                title="Available when the Programs & Groups feature is implemented"
              >
                Add to Group
              </button>
              <button
                className="customer-button customer-button--secondary"
                type="button"
                onClick={clearCustomerSelection}
              >
                Clear selection
              </button>
            </>
          ) : null}
          <button
            className="customer-button customer-button--secondary customer-add-button"
            ref={addCustomerButtonRef}
            type="button"
            onClick={() => {
              setNotice(null);
              setIsCreateDialogOpen(true);
            }}
          >
            Add New Customer
          </button>
        </div>
      </section>

      {notice !== null ? (
        <CustomerSuccessNotice message={notice} onDismiss={dismissNotice} />
      ) : null}

      <div className="customers-table-region">
        {listState.status === 'loading' ? (
          <div className="customer-state customer-state--loading" role="status">
            <span className="customer-spinner" aria-hidden="true" />
            <p>Loading customer directory…</p>
          </div>
        ) : null}

        {listState.status === 'error' ? (
          <div className="customer-state customer-state--error" role="alert">
            <strong>Customer directory unavailable</strong>
            <p>{listState.message}</p>
            <button
              className="customer-button customer-button--secondary"
              type="button"
              onClick={() => {
                setListState({ status: 'loading' });
                setRefreshKey((current) => current + 1);
              }}
            >
              Try again
            </button>
          </div>
        ) : null}

        {listData !== null && listData.items.length === 0 ? (
          <div className="customer-state customer-state--empty">
            <span className="customer-empty-icon" aria-hidden="true">◎</span>
            <strong>{search.length > 0 ? 'No matching customers' : 'No customers yet'}</strong>
            <p>
              {search.length > 0
                ? 'Try a different name or criteria.'
                : 'Create the first customer record to start the directory.'}
            </p>
            {search.length > 0 ? (
              <button
                className="customer-button customer-button--secondary"
                type="button"
                onClick={clearSearch}
              >
                Clear search
              </button>
            ) : null}
          </div>
        ) : null}

        {listData !== null && listData.items.length > 0 ? (
          <div className="customers-table-scroll">
            <table className="customers-table">
              <caption className="customer-sr-only">Active customers</caption>
              <colgroup>
                <col className="customer-col-selection" />
                <col className="customer-col-name" />
                <col className="customer-col-email" />
                <col className="customer-col-phone" />
                <col className="customer-col-birth-date" />
                <col className="customer-col-age" />
                <col className="customer-col-passport-expiry" />
                <col className="customer-col-passport-status" />
                <col className="customer-col-date-audit" />
              </colgroup>
              <thead>
                <tr>
                  <th className="customer-selection-cell" scope="col">
                    <input
                      ref={selectAllCustomersRef}
                      className="customer-selection-checkbox"
                      type="checkbox"
                      checked={allVisibleCustomersSelected}
                      aria-label="Select all customers on this page"
                      onChange={(event) => toggleVisibleCustomerSelection(event.target.checked)}
                    />
                  </th>
                  <th scope="col">Name</th>
                  <th scope="col">Email</th>
                  <th scope="col">Phone</th>
                  <th scope="col">Birth date</th>
                  <th scope="col">Age</th>
                  <th scope="col">Passport expires</th>
                  <th scope="col">Passport status</th>
                  <th scope="col">Last update</th>
                </tr>
              </thead>
              <tbody>
                {listData.items.map((customer) => (
                  <CustomerRow
                    key={customer.id}
                    customer={customer}
                    isSelected={selectedCustomerIds.includes(customer.id)}
                    onSelectionChange={() => toggleCustomerSelection(customer.id)}
                    onOpen={(trigger) => {
                      setNotice(null);
                      detailsTriggerRef.current = trigger;
                      setSelectedCustomerId(customer.id);
                    }}
                  />
                ))}
              </tbody>
            </table>
          </div>
        ) : null}
      </div>

      <footer className="customers-pagination">
        <p>
          {listData === null || listData.totalCount === 0
            ? 'No records to display'
            : `${(listData.page - 1) * listData.pageSize + 1}–${Math.min(
                listData.page * listData.pageSize,
                listData.totalCount,
              )} of ${listData.totalCount}`}
        </p>
        <div className="customers-pagination-controls">
          <button
            className="customer-button customer-button--secondary"
            type="button"
            disabled={listData === null || listData.page <= 1}
            onClick={() => {
              setListState({ status: 'loading' });
              setSelectedCustomerIds([]);
              setPage((current) => Math.max(1, current - 1));
            }}
          >
            Previous
          </button>
          <span aria-current="page">
            Page {listData?.page ?? page} of {Math.max(1, listData?.totalPages ?? 1)}
          </span>
          <button
            className="customer-button customer-button--secondary"
            type="button"
            disabled={
              listData === null
              || listData.totalPages === 0
              || listData.page >= listData.totalPages
            }
            onClick={() => {
              setListState({ status: 'loading' });
              setSelectedCustomerIds([]);
              setPage((current) => current + 1);
            }}
          >
            Next
          </button>
        </div>
      </footer>

      {isCreateDialogOpen ? (
        <CreateCustomerDialog
          onClose={() => {
            focusAfterDialogRef.current = addCustomerButtonRef.current;
            setIsCreateDialogOpen(false);
          }}
          onCreated={handleCreated}
        />
      ) : null}

      {selectedCustomerId !== null ? (
        <CustomerDetailsDialog
          key={selectedCustomerId}
          customerId={selectedCustomerId}
          onClose={() => {
            focusAfterDialogRef.current = detailsTriggerRef.current;
            setSelectedCustomerId(null);
          }}
          onChanged={handleChanged}
          onDeleted={handleDeleted}
        />
      ) : null}
    </section>
  );
}

function CustomerRow({
  customer,
  isSelected,
  onSelectionChange,
  onOpen,
}: Readonly<{
  customer: CustomerListItem;
  isSelected: boolean;
  onSelectionChange: () => void;
  onOpen: (trigger: HTMLButtonElement) => void;
}>) {
  const fullName = `${customer.firstName} ${customer.lastName}`;
  const passportStatus = customer.passportExpiresOn === null
    ? 'missing'
    : customer.isPassportValid
      ? 'valid'
      : 'review';

  return (
    <tr className={isSelected ? 'customers-table-row--selected' : undefined}>
      <td className="customer-selection-cell">
        <input
          className="customer-selection-checkbox"
          type="checkbox"
          checked={isSelected}
          aria-label={`Select ${fullName}`}
          onChange={onSelectionChange}
        />
      </td>
      <td>
        <button
          className="customer-name-button"
          type="button"
          aria-label={`View details for ${fullName}`}
          onClick={(event) => onOpen(event.currentTarget)}
        >
          <strong>{fullName}</strong>
        </button>
      </td>
      <td>{customer.email ?? <span className="customer-muted">Not provided</span>}</td>
      <td>{customer.phoneNumber ?? <span className="customer-muted">Not provided</span>}</td>
      <td>{formatDate(customer.dateOfBirth)}</td>
      <td>{customer.age ?? <span className="customer-muted">—</span>}</td>
      <td>{formatDate(customer.passportExpiresOn)}</td>
      <td>
        <span className={`customer-status customer-status--${passportStatus}`}>
          {passportStatus === 'valid' ? 'Valid' : passportStatus === 'review' ? 'Review' : 'Missing'}
        </span>
      </td>
      <td>
        {formatAuditDate(customer.updatedAt ?? customer.createdAt)}
      </td>
    </tr>
  );
}

function formatFullName(customer: CustomerDetails): string {
  return [customer.firstName, customer.middleName, customer.lastName]
    .filter((part): part is string => part !== null && part.trim().length > 0)
    .join(' ');
}
