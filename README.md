import frappe
import csv
import io
from frappe import _
from datetime import datetime
from frappe.utils import get_last_day, nowdate
from frappe import whitelist

@frappe.whitelist()
def importCsv():
    # Supprimer les headers Expect si présents
    if hasattr(frappe.local, 'request') and frappe.local.request:
        frappe.local.request.environ.pop('HTTP_EXPECT', None)
    
    file1 = frappe.request.files.get("file1")
    file2 = frappe.request.files.get("file2")
    file3 = frappe.request.files.get("file3")

    ref_map = {}

    try:
        # Utiliser une transaction plus simple
        frappe.db.begin()

        # --- Traitement du fichier des employés ---
        if file1:
            content = file1.read().decode('utf-8')
            csv_reader = csv.DictReader(io.StringIO(content))

            for row in csv_reader:
                ref = row.get("Ref")
                nom = row.get("Nom")
                prenom = row.get("Prenom")
                genre = row.get("genre", "").strip().lower()

                if genre == "masculin":
                    gender = "Male"
                elif genre == "feminin":
                    gender = "Female"
                else:
                    gender = "Male"  # Valeur par défaut au lieu de None

                date_embauche_str = row.get("Date embauche")
                try:
                    date_embauche = datetime.strptime(date_embauche_str, "%d/%m/%Y").date()
                except ValueError:
                    frappe.log_error(f"Date embauche invalide : {date_embauche_str}")
                    continue  # Passer à la ligne suivante au lieu de lever une exception

                date_naissance_str = row.get("date naissance")
                try:
                    date_naissance = datetime.strptime(date_naissance_str, "%d/%m/%Y").date()
                except ValueError:
                    frappe.log_error(f"Date naissance invalide : {date_naissance_str}")
                    continue

                company = row.get("company")

                # Holiday List (simplifié)
                holiday_name = frappe.db.get_value("Holiday List", {}, "name")
                if not holiday_name:
                    try:
                        holiday_list = frappe.get_doc({
                            "doctype": "Holiday List",
                            "holiday_list_name": f"Default Holidays {company}",
                            "from_date": frappe.utils.nowdate(),
                            "to_date": frappe.utils.add_days(frappe.utils.nowdate(), 365),
                        })
                        holiday_list.append("holidays", {
                            "holiday_date": frappe.utils.nowdate(),
                            "description": "Jour férié par défaut"
                        })
                        holiday_list.insert(ignore_permissions=True)
                        holiday_name = holiday_list.name
                    except Exception as e:
                        frappe.log_error(f"Erreur création Holiday List: {e}")
                        continue

                # Company (simplifié)
                if not frappe.db.exists("Company", company):
                    try:
                        new_company = frappe.new_doc("Company")
                        new_company.company_name = company
                        new_company.default_currency = "USD"
                        new_company.default_holiday_list = holiday_name
                        new_company.insert(ignore_permissions=True)
                    except Exception as e:
                        frappe.log_error(f"Erreur création Company: {e}")
                        continue

                # Employé
                try:
                    employee = frappe.get_doc({
                        "doctype": "Employee",
                        "first_name": nom,
                        "last_name": prenom,
                        "gender": gender,
                        "date_of_birth": date_naissance,
                        "date_of_joining": date_embauche,
                        "company": company,
                    })
                    employee.insert(ignore_permissions=True)
                    ref_map[ref] = employee.name
                except Exception as e:
                    frappe.log_error(f"Erreur création Employee {nom} {prenom}: {e}")
                    continue

        # --- Traitement du fichier des Salary Components ---
        if file2:
            try:
                content2 = file2.read().decode('utf-8')
                csv_reader2 = csv.DictReader(io.StringIO(content2))
                salary_structures = {}

                for row in csv_reader2:
                    try:
                        salary_structure = row.get("salary structure", "").strip()
                        name = row.get("name", "").strip()
                        abbr = row.get("Abbr", "").strip()
                        comp_type = row.get("type", "").strip().lower()
                        valeur = row.get("valeur", "").strip()
                        company = row.get("company", "").strip()

                        if not all([salary_structure, name, comp_type, company]):
                            continue

                        # Création du Salary Component
                        if not frappe.db.exists("Salary Component", name):
                            comp = frappe.new_doc("Salary Component")
                            comp.salary_component = name
                            comp.type = comp_type.title()
                            comp.company = company
                            comp.is_tax_applicable = (valeur.lower() == "base")
                            comp.depends_on_payment_days = 0
                            comp.insert(ignore_permissions=True, ignore_mandatory=True)

                        # Préparer les components pour la Salary Structure
                        if salary_structure not in salary_structures:
                            salary_structures[salary_structure] = {
                                "company": company,
                                "earnings": [],
                                "deductions": []
                            }

                        component_data = {
                            "salary_component": name,
                            "abbr": abbr,
                            "amount_based_on_formula": 1,
                            "formula": valeur
                        }

                        if comp_type == "earning":
                            salary_structures[salary_structure]["earnings"].append(component_data)
                        elif comp_type == "deduction":
                            salary_structures[salary_structure]["deductions"].append(component_data)

                    except Exception as e:
                        frappe.log_error(f"Erreur traitement Salary Component: {e}")
                        continue

                # Création des Salary Structures
                for structure_name, structure_data in salary_structures.items():
                    try:
                        if frappe.db.exists("Salary Structure", structure_name):
                            ss = frappe.get_doc("Salary Structure", structure_name)
                        else:
                            ss = frappe.new_doc("Salary Structure")
                            ss.name = structure_name
                            ss.salary_structure_name = structure_name
                            ss.company = structure_data["company"]
                            ss.payroll_frequency = "Monthly"

                        for earning in structure_data["earnings"]:
                            if not any(e.salary_component == earning["salary_component"] for e in ss.earnings):
                                ss.append("earnings", earning)

                        for deduction in structure_data["deductions"]:
                            if not any(d.salary_component == deduction["salary_component"] for d in ss.deductions):
                                ss.append("deductions", deduction)

                        ss.save(ignore_permissions=True)
                        if ss.docstatus == 0:
                            ss.submit()

                    except Exception as e:
                        frappe.log_error(f"Erreur Salary Structure {structure_name}: {e}")
                        continue

            except Exception as e:
                frappe.log_error(f"Erreur générale file2: {e}")

        # --- Traitement du fichier des Salary Structure Assignments ---
        if file3:
            try:
                content3 = file3.read().decode('utf-8')
                csv_reader3 = csv.DictReader(io.StringIO(content3))

                for row in csv_reader3:
                    try:
                        mois_str = row.get("Mois", "").strip()
                        ref_employe = row.get("Ref Employe", "").strip()
                        salaire_base_str = row.get("Salaire Base", "0").strip()
                        salary_structure = row.get("Salaire", "").strip()

                        if not all([mois_str, ref_employe, salary_structure]):
                            continue

                        # Validation des données
                        mois = datetime.strptime(mois_str, "%d/%m/%Y").date()
                        salaire_base = float(salaire_base_str) if salaire_base_str else 0
                        
                        employee_id = ref_map.get(ref_employe)
                        if not employee_id:
                            frappe.log_error(f"Référence employé '{ref_employe}' non trouvée.")
                            continue

                        employee_doc = frappe.get_doc("Employee", employee_id)
                        company = employee_doc.company
                        currency = frappe.db.get_value("Company", company, "default_currency")

                        # Création SSA
                        existing_ssa = frappe.db.exists("Salary Structure Assignment", {
                            "employee": employee_id,
                            "from_date": mois,
                            "salary_structure": salary_structure
                        })

                        if not existing_ssa:
                            ssa = frappe.new_doc("Salary Structure Assignment")
                            ssa.employee = employee_id
                            ssa.from_date = mois
                            ssa.salary_structure = salary_structure
                            ssa.base = salaire_base
                            ssa.company = company
                            ssa.currency = currency
                            ssa.insert(ignore_permissions=True)
                            ssa.submit()

                        # Création Salary Slip
                        salary_slip = frappe.new_doc("Salary Slip")
                        salary_slip.employee = employee_id
                        salary_slip.posting_date = nowdate()
                        salary_slip.company = company
                        salary_slip.currency = currency
                        salary_slip.payroll_frequency = "Monthly"
                        salary_slip.salary_structure = salary_structure
                        salary_slip.start_date = mois
                        salary_slip.end_date = get_last_day(mois)
                        salary_slip.insert(ignore_permissions=True)
                        salary_slip.submit()

                    except Exception as e:
                        frappe.log_error(f"Erreur SSA/Salary Slip: {e}")
                        continue

            except Exception as e:
                frappe.log_error(f"Erreur générale file3: {e}")

        # Validation de la transaction
        frappe.db.commit()
        
        return {
            "message": "Import terminé avec succès",
            "ref_map": ref_map,
            "status": "success"
        }

    except Exception as e:
        frappe.db.rollback()
        frappe.log_error(frappe.get_traceback(), "Erreur Import CSV")
        
        return {
            "message": f"Erreur lors de l'import : {str(e)}",
            "status": "error"
        }